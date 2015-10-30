/*
 * Phosphorus Five, copyright 2014 - 2015, Thomas Hansen, phosphorusfive@gmail.com
 * Phosphorus Five is licensed under the terms of the MIT license, see the enclosed LICENSE file for details
 */

using System.IO;
using p5.core;
using p5.exp;

namespace p5.file.file
{
    /// <summary>
    ///     Class to help save files.
    /// 
    ///     Contains [p5.file.save], [p5.file.binary.save], and their associated helper methods.
    /// </summary>
    public static class Save
    {
        /// <summary>
        ///     Saves zero or more text files to disc.
        /// 
        ///     The files to save, should be given as either [source], [src], [rel-source] or [rel-src]. If 
        ///     you save one file, using a constant as the file path, then you must use [source] or [src], which
        ///     are synonyms btw. If you use an expression pointing to your file path(s), then you can use a relative
        ///     source, if you wish, by either using [rel-source] or [rel-src] pointing to the actual contents to be saved.
        /// 
        ///     Example that saves a file with the name of "foo.txt" and the contents of "Howdy World";
        /// 
        ///     <pre>p5.file.save:foo.txt
        ///   source:Howdy World</pre>
        /// 
        ///     Example that saves two files; "foo1.txt" and "foo2.txt", with the contents of "Hello World 1.0" and "Hello World 2.0";
        /// 
        ///     <pre>
        /// _data
        ///   foo1.txt:Howdy World 1.0
        ///   foo2.txt:Howdy World 2.0
        /// p5.file.save:@/-/*?name
        ///   rel-source:@?value</pre>
        /// </summary>
        /// <param name="context">Application context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "p5.file.save")]
        private static void p5_file_save (ApplicationContext context, ActiveEventArgs e)
        {
            // getting root folder
            var rootFolder = Common.GetRootFolder (context);

            if (e.Args.LastChild.Name == "source" || e.Args.LastChild.Name == "src") {

                // static source
                var source = Utilities.Convert<string> (XUtil.SourceSingle (e.Args, context), context);

                // iterating through each file path given
                foreach (var idx in XUtil.Iterate<string> (e.Args, context)) {
                    // saving source to currenly iterated file
                    using (TextWriter writer = File.CreateText (rootFolder + idx)) {
                        writer.Write (source);
                    }
                }
            } else if (e.Args.LastChild.Name == "rel-source" || e.Args.LastChild.Name == "rel-src") {

                // relative source
                foreach (var idx in XUtil.Iterate (e.Args, context)) {
                    using (TextWriter writer = File.CreateText (rootFolder + idx.Value)) {
                        // finding source relative to destination
                        var source = Utilities.Convert<string> (XUtil.SourceSingle (e.Args, idx.Node, context), context);
                        writer.Write (source);
                    }
                }
            }
        }

        /// <summary>
        ///     Saves zero or more binary files to disc.
        /// 
        ///     The files to save, should be given as either [source], [src], [rel-source] or [rel-src]. If 
        ///     you save one file, using a constant as the file path, then you must use [source] or [src], which
        ///     are synonyms btw. If you use an expression pointing to your file path(s), then you can use a relative
        ///     source, if you wish, by either using [rel-source] or [rel-src] pointing to the actual contents to be saved.
        /// 
        ///     Example that saves a file with the name of "foo.png"
        /// 
        ///     <pre>_data
        ///   _1:iVBORw0KGgoAAAANSUhEUgAAAAUAAAAICAYAAAAx8TU7AAAABmJLR0QAAAAAAAD5Q7t/
        ///   _2:AAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH3wMRESMW1yBFBwAAADBJREFUCNeNjj
        ///   _3:EKACAQw1Lx/1+Og9wJ4mC2BloaQC4mgB6fZMsKxeBBS7Vn/ut5XVoUIQsTo7AhfAAAAABJRU5ErkJggg==
        /// p5.file.binary.save:foo.png
        ///   source:@/./-/*?value</pre>
        /// <param name="context">Application context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "p5.file.binary.save")]
        private static void p5_file_binary_save (ApplicationContext context, ActiveEventArgs e)
        {
            // getting root folder
            var rootFolder = Common.GetRootFolder (context);

            if (e.Args.LastChild.Name == "source" || e.Args.LastChild.Name == "src") {

                // static source
                var source = Utilities.Convert<byte[]> (XUtil.SourceSingle (e.Args, context), context);

                // iterating through each file path given
                foreach (var idx in XUtil.Iterate<string> (e.Args, context)) {
                    // saving source to currenly iterated file
                    using (FileStream stream = File.Create (rootFolder + idx)) {
                        stream.Write (source, 0, source.Length);
                    }
                }
            } else if (e.Args.LastChild.Name == "rel-source" || e.Args.LastChild.Name == "rel-src") {

                // relative source
                foreach (var idx in XUtil.Iterate (e.Args, context)) {
                    using (FileStream stream = File.Create (rootFolder + idx)) {
                        // finding source relative to destination
                        var source = Utilities.Convert<byte[]> (XUtil.SourceSingle (e.Args, idx.Node, context), context);
                        stream.Write (source, 0, source.Length);
                    }
                }
            }
        }
    }
}