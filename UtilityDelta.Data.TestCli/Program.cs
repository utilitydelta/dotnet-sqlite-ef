using System;
using System.Linq;

namespace UtilityDelta.Data.TestCli
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            using (var mgr = new ContextManager("test1.db"))
            {
                AddCommentToBlogExisting(mgr);

                var cmt = new Tag
                {
                    BlogId = 1,
                    Name = "hello world"
                };
                mgr.Context.Tags.Add(cmt);
                mgr.Context.SaveChanges();
            }
        }

        private static void AddCommentToBlogExisting(ContextManager mgr)
        {
            var blog = mgr.Context.Blogs.FirstOrDefault();

            if (blog == null)
            {
                blog = new Blog
                {
                    Url = "/blah/bh"
                };
                mgr.Context.Blogs.Add(blog);
            }

            blog.Tags.Add(new Tag
            {
                Name = "Some comment!"
            });

            mgr.Context.SaveChanges();
        }
    }
}