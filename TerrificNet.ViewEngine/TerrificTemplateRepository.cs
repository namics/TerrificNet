using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TerrificNet.ViewEngine.Config;

namespace TerrificNet.ViewEngine
{
	public class TerrificTemplateRepository : ITemplateRepository
	{
		private const string HtmlExtension = "html";

		private readonly ITerrificNetConfig _config;
	    private readonly IFileSystem _fileSystem;

	    public TerrificTemplateRepository(ITerrificNetConfig config, IFileSystem fileSystem)
		{
		    _config = config;
		    _fileSystem = fileSystem;
		}

	    public bool TryGetTemplate(string id, string skin, out TemplateInfo templateInfo)
		{
		    var fileName = id;
			if (!string.IsNullOrEmpty(skin))
				fileName += "-" + skin;

		    var templates = GetAll().ToDictionary(f => f.Id, f => f);
		    return templates.TryGetValue(fileName, out templateInfo);
		}

	    private IEnumerable<TemplateInfo> Read(string directory)
	    {
	        if (!_fileSystem.DirectoryExists(directory))
	            return Enumerable.Empty<TemplateInfo>();

			return _fileSystem.DirectoryGetFiles(directory, "html").Select(f =>
			{
			    var relativePath = GetTemplateId(f).TrimStart('/');
                return new FileTemplateInfo(relativePath, f, _fileSystem); 
            });
	    }

	    private string GetTemplateId(string info)
	    {
	        return _fileSystem.Path.Combine(_fileSystem.Path.GetDirectoryName(info), _fileSystem.Path.GetFileNameWithoutExtension(info));
	    }

	    public IEnumerable<TemplateInfo> GetAll()
	    {
	        return Read(_config.ViewPath)
                .Union(Read(_config.ModulePath));
	    }
	}
}