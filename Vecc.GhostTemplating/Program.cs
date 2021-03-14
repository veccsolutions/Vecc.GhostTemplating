using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Vecc.GhostTemplating.Client;
using Vecc.GhostTemplating.RazorSupport;

namespace Vecc.GhostTemplating
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var servicesMain = ServiceProvider.Build(args);
            using var serviceScope = servicesMain.CreateScope();
            var services = serviceScope.ServiceProvider;

            var logger = services.GetRequiredService<ILogger<Program>>();
            try
            {
                logger.LogInformation("Starting");
                var client = services.GetRequiredService<GhostClient>();
                var engine = services.GetRequiredService<IRazorViewEngine>();
                var templatingOptions = services.GetRequiredService<IOptions<TemplatingOptions>>();
                var options = templatingOptions.Value;
                logger.LogInformation("Options: {@options}", options);

                var settings = await client.GetSettingsAsync();
                var tags = await client.GetTagsAsync();
                var authors = await client.GetAuthorsAsync();
                var pages = await client.GetPagesAsync();
                var posts = await client.GetPostsAsync();

                Sanitize(settings);
                tags = Sanitize(tags, settings);
                posts = Sanitize(posts, settings);
                pages = Sanitize(pages, settings);
                Sanitize(authors, settings);

                //write out the pages
                foreach (var page in pages)
                {
                    await HandlePostAsync(page, settings, options, services, engine, "page");
                }

                foreach (var post in posts)
                {
                    await HandlePostAsync(post, settings, options, services, engine, "post");
                }

                foreach (var author in authors)
                {
                    await HandleAuthorAsync(author, posts, settings, options, services, engine);
                }

                foreach (var tag in tags)
                {
                    await HandleTagAsync(tag, posts, settings, options, services, engine);
                }

                await HandleIndexPageAsync(posts, settings, options, services, engine);
                CopyAssets("Assets", Path.Combine(options.OutputDirectory, "assets"));
                BuildRssFeed(posts, authors, tags, settings, options, services, engine);
                BuildSitemap(pages, posts, authors, tags, settings, options);
                await GetFavoriteIconAsync(settings, options);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error while generating site");
            }
            serviceScope.Dispose();
        }

        private static async Task HandlePostAsync(Post post, Settings settings, TemplatingOptions options, IServiceProvider services, IRazorViewEngine engine, string type)
        {
            var httpContext = new DefaultHttpContext { RequestServices = services };
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var modelStateDictionary = new ModelStateDictionary();
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var tempDataProvider = new VirtualTempDataProvider();
            var htmlHelperOptions = new HtmlHelperOptions();

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor, modelStateDictionary);
            var viewDataDictionary = new ViewDataDictionary(modelMetadataProvider, modelStateDictionary);
            var tempDataDictionary = new TempDataDictionary(httpContext, tempDataProvider);
            viewDataDictionary.Model = post;

            using (var stringWriter = new StringWriter())
            {
                var view = engine.GetView(".", $"/Templates/{type}.cshtml", true);
                var viewContext = new ViewContext(actionContext, view.View, viewDataDictionary, tempDataDictionary, stringWriter, htmlHelperOptions);

                await view.View.RenderAsync(viewContext);

                var result = stringWriter.ToString();
                var directory = Path.Combine(options.OutputDirectory, post.Slug);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(Path.Combine(directory, "index.html"), result);
                await DownloadImagesFromPostAsync(post.HTML, settings, options);
                if (type == "post")
                {
                    //amp it
                    view = engine.GetView(".", $"/Templates/post-amp.cshtml", true);
                    viewContext = new ViewContext(actionContext, view.View, viewDataDictionary, tempDataDictionary, stringWriter, htmlHelperOptions);

                    await view.View.RenderAsync(viewContext);

                    result = stringWriter.ToString();
                    directory = Path.Combine(directory, "amp");
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllText(Path.Combine(directory, "index.html"), result);
                }
            }
        }

        private static async Task HandleAuthorAsync(Author author, Post[] posts, Settings settings, TemplatingOptions options, IServiceProvider services, IRazorViewEngine engine)
        {
            var postsToDisplay = 27;
            var httpContext = new DefaultHttpContext { RequestServices = services };
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var modelStateDictionary = new ModelStateDictionary();
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var tempDataProvider = new VirtualTempDataProvider();
            var htmlHelperOptions = new HtmlHelperOptions();
            var imageDirectory = Path.Combine(options.OutputDirectory, "content", "images", "authors", author.Slug);
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor, modelStateDictionary);
            var viewDataDictionary = new ViewDataDictionary(modelMetadataProvider, modelStateDictionary);
            var tempDataDictionary = new TempDataDictionary(httpContext, tempDataProvider);

            posts = posts.Where(x => x.Authors.Any(x => x.Id == author.Id)).OrderByDescending(x => x.PublishedAt).ToArray();

            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }

            if (!string.IsNullOrWhiteSpace(author.CoverImage))
            {
                using var client = new HttpClient();
                var imageResult = await client.GetAsync(author.CoverImage);

                if (imageResult.IsSuccessStatusCode)
                {
                    using var imageStream = await imageResult.Content.ReadAsStreamAsync();
                    using var memoryStream = new MemoryStream();
                    await imageStream.CopyToAsync(memoryStream);

                    var widths = new[] { 600, 1000, 2000 };
                    foreach (var width in widths)
                    {
                        memoryStream.Position = 0;
                        using var image = await Image.LoadAsync(memoryStream);
                        image.Mutate(context => context.Resize(width, 0));
                        await image.SaveAsPngAsync(Path.Combine(imageDirectory, width + ".png"));
                    }
                }
                else
                {
                    author.CoverImage = null;
                }
            }
            viewDataDictionary.Model = author;

            var batch = 1;
            while (posts.Length > 0)
            {
                var directory = Path.Combine(options.OutputDirectory, "author", author.Slug);
                if (batch > 1)
                {
                    directory = Path.Combine(directory, batch.ToString());
                }

                if (batch == 1)
                {
                    author.Previous = null;
                }
                else if (batch == 2)
                {
                    author.Previous = author.Url;
                }
                else
                {
                    author.Previous = author.Url + (batch - 1) + "/";
                }

                if (posts.Length > postsToDisplay)
                {
                    author.Next = author.Url + (batch + 1) + "/";
                }
                else
                {
                    author.Next = null;
                }

                using (var stringWriter = new StringWriter())
                {
                    var view = engine.GetView(".", $"/Templates/author.cshtml", true);
                    var viewContext = new ViewContext(actionContext, view.View, viewDataDictionary, tempDataDictionary, stringWriter, htmlHelperOptions);

                    author.PostsToDisplay = posts.Take(postsToDisplay).ToArray();
                    await view.View.RenderAsync(viewContext);

                    var result = stringWriter.ToString();

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllText(Path.Combine(directory, "index.html"), result);
                }

                posts = posts.Skip(postsToDisplay).ToArray();
                batch++;
            }

        }

        private static async Task HandleTagAsync(Tag tag, Post[] posts, Settings settings, TemplatingOptions options, IServiceProvider services, IRazorViewEngine engine)
        {
            var postsToDisplay = 27;
            var httpContext = new DefaultHttpContext { RequestServices = services };
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var modelStateDictionary = new ModelStateDictionary();
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var tempDataProvider = new VirtualTempDataProvider();
            var htmlHelperOptions = new HtmlHelperOptions();

            posts = posts.Where(x => x.Tags.Any(x => x.Id == tag.Id)).OrderByDescending(x => x.PublishedAt).ToArray();
            tag.PostCount = posts.Length;

            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor, modelStateDictionary);
            var viewDataDictionary = new ViewDataDictionary(modelMetadataProvider, modelStateDictionary);
            var tempDataDictionary = new TempDataDictionary(httpContext, tempDataProvider);
            viewDataDictionary.Model = tag;

            var batch = 1;
            while (posts.Length > 0)
            {
                var directory = Path.Combine(options.OutputDirectory, "tag", tag.Slug);
                if (batch > 1)
                {
                    directory = Path.Combine(directory, batch.ToString());
                }

                if (batch == 1)
                {
                    tag.Previous = null;
                }
                else if (batch == 2)
                {
                    tag.Previous = tag.Url;
                }
                else
                {
                    tag.Previous = tag.Url + (batch - 1) + "/";
                }

                if (posts.Length > postsToDisplay)
                {
                    tag.Next = tag.Url + (batch + 1) + "/";
                }
                else
                {
                    tag.Next = null;
                }

                using (var stringWriter = new StringWriter())
                {
                    var view = engine.GetView(".", $"/Templates/tag.cshtml", true);
                    var viewContext = new ViewContext(actionContext, view.View, viewDataDictionary, tempDataDictionary, stringWriter, htmlHelperOptions);

                    tag.PostsToDisplay = posts.Take(postsToDisplay).ToArray();
                    await view.View.RenderAsync(viewContext);

                    var result = stringWriter.ToString();

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllText(Path.Combine(directory, "index.html"), result);
                }

                posts = posts.Skip(postsToDisplay).ToArray();
                batch++;
            }
        }

        private static async Task HandleIndexPageAsync(Post[] posts, Settings settings, TemplatingOptions options, IServiceProvider services, IRazorViewEngine engine)
        {
            var postsToDisplay = 27;
            var httpContext = new DefaultHttpContext { RequestServices = services };
            var routeData = new RouteData();
            var actionDescriptor = new ActionDescriptor();
            var modelStateDictionary = new ModelStateDictionary();
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var tempDataProvider = new VirtualTempDataProvider();
            var htmlHelperOptions = new HtmlHelperOptions();
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor, modelStateDictionary);
            var viewDataDictionary = new ViewDataDictionary(modelMetadataProvider, modelStateDictionary);
            var tempDataDictionary = new TempDataDictionary(httpContext, tempDataProvider);
            var imageDirectory = Path.Combine(options.OutputDirectory, "content", "images", "index");
            var index = new IndexPage
            {
                Options = options,
                Settings = settings
            };

            posts = posts.OrderByDescending(x => x.PublishedAt).ToArray();

            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }

            if (!string.IsNullOrWhiteSpace(settings.CoverImage))
            {
                using var client = new HttpClient();
                var imageResult = await client.GetAsync(settings.CoverImage);

                if (imageResult.IsSuccessStatusCode)
                {
                    using var imageStream = await imageResult.Content.ReadAsStreamAsync();
                    using var memoryStream = new MemoryStream();
                    await imageStream.CopyToAsync(memoryStream);

                    var widths = new[] { 600, 1000, 2000 };
                    foreach (var width in widths)
                    {
                        memoryStream.Position = 0;
                        using var image = await Image.LoadAsync(memoryStream);

                        index.ImageHeight = image.Height;
                        index.ImageWidth = image.Width;

                        image.Mutate(context => context.Resize(width, 0));
                        await image.SaveAsPngAsync(Path.Combine(imageDirectory, width + ".png"));
                    }
                }
            }

            viewDataDictionary.Model = index;

            var batch = 1;
            while (posts.Length > 0)
            {
                var directory = options.OutputDirectory;

                if (batch > 1)
                {
                    directory = Path.Combine(directory, "index", batch.ToString());
                }

                if (batch == 1)
                {
                    index.Previous = null;
                }
                else if (batch == 2)
                {
                    index.Previous = "/";
                }
                else
                {
                    index.Previous = "/index/" + (batch - 1) + "/";
                }

                if (posts.Length > postsToDisplay)
                {
                    index.Next = "/index/" + (batch + 1) + "/";
                }
                else
                {
                    index.Next = null;
                }

                using (var stringWriter = new StringWriter())
                {
                    var view = engine.GetView(".", $"/Templates/index.cshtml", true);
                    var viewContext = new ViewContext(actionContext, view.View, viewDataDictionary, tempDataDictionary, stringWriter, htmlHelperOptions);

                    index.PostsToDisplay = posts.Take(postsToDisplay).ToArray();
                    await view.View.RenderAsync(viewContext);

                    var result = stringWriter.ToString();

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.WriteAllText(Path.Combine(directory, "index.html"), result);
                }

                posts = posts.Skip(postsToDisplay).ToArray();
                batch++;
            }
        }

        private static HttpClient _imageClient = new HttpClient();
        private static async Task DownloadImagesFromPostAsync(string post, Settings settings, TemplatingOptions options)
        {
            //this straight sucks
            var imageRegex = new Regex("<img.+?>");
            var tagMatches = imageRegex.Matches(post);
            if (tagMatches.Count > 0)
            {
                foreach (Match tagMatch in tagMatches)
                {
                    var quoteRegex = new Regex("\".+?\"");
                    var quoteMatches = quoteRegex.Matches(tagMatch.Value);
                    foreach (Match quoteMatch in quoteMatches)
                    {
                        var splits = quoteMatch.Value.Split(' ');
                        foreach (var split in splits)
                        {
                            var item = split.Trim('\"');
                            if (item.StartsWith(settings.Url))
                            {
                                //we have an image, download it.
                                var imageResult = await _imageClient.GetAsync(item);
                                if (imageResult.IsSuccessStatusCode)
                                {
                                    var filePath = item.Replace(settings.Url, "");
                                    var destinationFile = Path.Combine(options.OutputDirectory, filePath);
                                    var destination = new System.IO.FileInfo(destinationFile);
                                    var destinationDirectory = destination.Directory.FullName;
                                    if (!Directory.Exists(destinationDirectory))
                                    {
                                        Directory.CreateDirectory(destinationDirectory);
                                    }
                                    var image = await imageResult.Content.ReadAsByteArrayAsync();
                                    if (File.Exists(destinationFile))
                                    {
                                        File.Delete(destinationFile);
                                    }
                                    File.WriteAllBytes(destinationFile, image);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void CopyAssets(string source, string target)
        {
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }

            foreach (var filename in Directory.GetFiles(source))
            {
                var file = new System.IO.FileInfo(filename);
                file.CopyTo(Path.Combine(target, file.Name), true);
                if (file.Name.EndsWith(".css", StringComparison.InvariantCultureIgnoreCase))
                {
                    var contents = File.ReadAllText(file.FullName);
                    while (contents.Contains("\r "))
                    {
                        contents = contents.Replace("\r ", "\r");
                    }
                    while (contents.Contains("\n "))
                    {
                        contents = contents.Replace("\n ", "\n");
                    }

                    contents = contents.Replace("\r", "");
                    contents = contents.Replace("\n", "");

                    File.WriteAllText(file.FullName, contents);
                }
            }
        }

        private static void BuildRssFeed(Post[] posts, Author[] authors, Tag[] tags, Settings settings, TemplatingOptions options, IServiceProvider services, IRazorViewEngine engine)
        {
            var targetDirectory = Path.Combine(options.OutputDirectory, "rss");
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            var feedItems = new List<SyndicationItem>();
            var oldestPost = DateTime.Now.AddMonths(-6);

            foreach (var post in posts.Where(x => x.PublishedAt > oldestPost))
            {
                var item = new SyndicationItem();
                foreach (var author in post.Authors)
                {
                    item.Authors.Add(new SyndicationPerson(string.Empty, author.Name, settings.Url + "author/" + author.Slug));
                }
                item.BaseUri = new Uri(post.Settings.Url + post.Slug + "/");
                item.Content = new TextSyndicationContent(post.HTML, TextSyndicationContentKind.Html);
                item.Copyright = new TextSyndicationContent(options.FormattedCopyright, TextSyndicationContentKind.Plaintext);
                item.Id = post.UUID;
                item.LastUpdatedTime = post.UpdatedAt;
                item.PublishDate = post.PublishedAt.Value;
                item.Summary = new TextSyndicationContent(post.MetaDescription ?? post.CustomExcerpt ?? post.Excerpt);

                foreach (var tag in post.Tags)
                {
                    item.Categories.Add(new SyndicationCategory(tag.Name));
                }

                item.Title = new TextSyndicationContent(post.Title);
                feedItems.Add(item);
            }

            var feed = new SyndicationFeed(feedItems);

            foreach (var author in authors)
            {
                var person = new SyndicationPerson(string.Empty, author.Name, settings.Url + "author/" + author.Slug);
                feed.Authors.Add(person);
                feed.Contributors.Add(person);
            }

            foreach (var tag in tags)
            {
                feed.Categories.Add(new SyndicationCategory(tag.Name));
            }

            feed.Copyright = new TextSyndicationContent(options.FormattedCopyright, TextSyndicationContentKind.Plaintext);
            feed.Description = new TextSyndicationContent(settings.Description, TextSyndicationContentKind.Plaintext);
            feed.TimeToLive = options.RssTimeToLive;
            feed.Generator = options.GeneratorName;
            feed.ImageUrl = new Uri(settings.Url + "favicon.png");
            feed.LastUpdatedTime = DateTime.UtcNow;
            feed.Title = new TextSyndicationContent(settings.Title, TextSyndicationContentKind.Plaintext);
            using var memoryStream = new MemoryStream();
            using var xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings
            {
                Indent = true
            });
            feed.SaveAsRss20(xmlWriter);
            xmlWriter.Flush();
            File.WriteAllBytes(Path.Combine(targetDirectory, "index.html"), memoryStream.ToArray());
        }

        private static void BuildSitemap(Page[] pages, Post[] posts, Author[] authors, Tag[] tags, Settings settings, TemplatingOptions options)
        {
            //build the index that ties the maps together
            var siteIndex = new Sitemap.sitemapindex
            {
                sitemap = GetArray(
                    new Sitemap.tSitemap { lastmod = DateTime.UtcNow.ToString("O"), loc = settings.Url + "sitemaps/pages.xml" },
                    new Sitemap.tSitemap { lastmod = DateTime.UtcNow.ToString("O"), loc = settings.Url + "sitemaps/posts.xml" },
                    new Sitemap.tSitemap { lastmod = DateTime.UtcNow.ToString("O"), loc = settings.Url + "sitemaps/authors.xml" },
                    new Sitemap.tSitemap { lastmod = DateTime.UtcNow.ToString("O"), loc = settings.Url + "sitemaps/tags.xml" })
            };
            File.WriteAllBytes(Path.Combine(options.OutputDirectory, "sitemap.xml"), XmlSerialize(siteIndex));
            var sitemapDirectory = Path.Combine(options.OutputDirectory, "sitemaps");
            if (!Directory.Exists(sitemapDirectory))
            {
                Directory.CreateDirectory(sitemapDirectory);
            }

            var map = new Sitemap.urlset
            {
                url = pages.Select(page => new Sitemap.tUrl
                {
                    changefreq = Sitemap.tChangeFreq.daily,
                    changefreqSpecified = true,
                    lastmod = page.UpdatedAt.ToString("O"),
                    loc = settings.Url + page.Slug + "/"
                }).ToArray()
            };
            File.WriteAllBytes(Path.Combine(sitemapDirectory, "pages.xml"), XmlSerialize(map));

            map = new Sitemap.urlset
            {
                url = posts.Select(post => new Sitemap.tUrl
                {
                    changefreq = Sitemap.tChangeFreq.daily,
                    changefreqSpecified = true,
                    lastmod = post.UpdatedAt.ToString("O"),
                    loc = settings.Url + post.Slug + "/"
                }).ToArray()
            };
            File.WriteAllBytes(Path.Combine(sitemapDirectory, "posts.xml"), XmlSerialize(map));

            map = new Sitemap.urlset
            {
                url = authors.Select(author => new Sitemap.tUrl
                {
                    changefreq = Sitemap.tChangeFreq.daily,
                    changefreqSpecified = true,
                    lastmod = DateTime.UtcNow.ToString("O"),
                    loc = settings.Url + "authors/" + author.Slug + "/"
                }).ToArray()
            };
            File.WriteAllBytes(Path.Combine(sitemapDirectory, "authors.xml"), XmlSerialize(map));

            map = new Sitemap.urlset
            {
                url = tags.Select(tag => new Sitemap.tUrl
                {
                    changefreq = Sitemap.tChangeFreq.daily,
                    changefreqSpecified = true,
                    lastmod = DateTime.UtcNow.ToString("O"),
                    loc = settings.Url + "tag/" + tag.Slug + "/"
                }).ToArray()
            };
            File.WriteAllBytes(Path.Combine(sitemapDirectory, "tags.xml"), XmlSerialize(map));
        }

        private static async Task GetFavoriteIconAsync(Settings settings, TemplatingOptions options)
        {
            var iconUrl = settings.Icon;
            if (string.IsNullOrWhiteSpace(settings.Icon))
            {
                iconUrl = settings.Url + "favicon.ico";
            }
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(iconUrl);
            if (response.IsSuccessStatusCode)
            {
                var iconBytes = await response.Content.ReadAsByteArrayAsync();
                var path = iconUrl.Replace(settings.Url, "");
                path = Path.Combine(options.OutputDirectory, path);
                var fileInfo = new System.IO.FileInfo(path);
                var directory = fileInfo.Directory;
                if (!directory.Exists)
                {
                    directory.Create();
                }
                File.WriteAllBytes(path, iconBytes);
            }
        }

        private static byte[] XmlSerialize<T>(T o)
        {
            using var stream = new MemoryStream();
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(stream, o);
            var result = stream.ToArray();
            return result;
        }

        private static void Sanitize(Settings settings)
        {
            foreach (var nav in settings.Navigation)
            {
                nav.Url = nav.Url.Replace(settings.Url, "/");
            }
        }

        private static Tag[] Sanitize(Tag[] tags, Settings settings)
        {
            tags = tags.Where(x => x.Visibility == "public").ToArray();

            foreach (var tag in tags)
            {
                tag.Url = tag.Url.Replace(settings.Url, "/");
                tag.Settings = settings;
            }

            return tags;
        }

        private static Page[] Sanitize(Page[] pages, Settings settings)
        {
            pages = pages.Where(x => x.PublishedAt.HasValue).ToArray();

            foreach (var page in pages)
            {
                Sanitize(page.Authors, settings);
                page.Settings = settings;
                page.Tags = Sanitize(page.Tags, settings);
                page.Url = page.Url.Replace(settings.Url, "/");
            }

            return pages;
        }

        private static Post[] Sanitize(Post[] posts, Settings settings)
        {
            posts = posts.Where(x => x.PublishedAt.HasValue).ToArray();

            foreach (var post in posts)
            {
                post.Tags = Sanitize(post.Tags, settings);
                post.Settings = settings;
            }

            return posts;
        }

        private static void Sanitize(Author[] authors, Settings settings)
        {
            foreach (var author in authors)
            {
                author.Url = author.Url.Replace(settings.Url, "/");
                author.Settings = settings;
            }
        }

        private static T[] GetArray<T>(params T[] items) => items;
    }
}
