﻿using System.IO;
using Microsoft.Ajax.Utilities;
using System.Threading.Tasks;

namespace TerrificNet.AssetCompiler.Compiler
{
    public class JsAssetCompiler : IAssetCompiler
    {
        /// <summary>
        /// (Awaitable) Compiles content with the give configuration (files and minify flag).
        /// </summary>
        /// <param name="content">Content to Compile</param>
        /// <returns>string with compiled content</returns>
        public Task<string> CompileAsync(string content)
        {
            var minifier = new Minifier();
            return Task.FromResult(minifier.MinifyJavaScript(content));
        }

        public bool CanProcess(string filename)
        {
            return Path.HasExtension(filename) && Path.GetExtension(filename) == ".js";
        }
    }
}
