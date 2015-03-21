﻿using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TerrificNet.ViewEngine;
using TerrificNet.ViewEngine.Config;
using TerrificNet.ViewEngine.IO;
using TerrificNet.ViewEngine.TemplateHandler;

namespace TerrificNet.Controllers
{
    public class TemplateController : TemplateControllerBase
    {
        private readonly IViewEngine _viewEngine;
        private readonly IModelProvider _modelProvider;
        private readonly ITemplateRepository _templateRepository;
        private readonly TerrificViewDefinitionRepository _viewRepository;

        public TemplateController(IViewEngine viewEngine, IModelProvider modelProvider, ITemplateRepository templateRepository, TerrificViewDefinitionRepository viewRepository)
        {
            _viewEngine = viewEngine;
            _modelProvider = modelProvider;
            _templateRepository = templateRepository;
            _viewRepository = viewRepository;
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Get(string path, string data = null)
        {
            IView view = null;
            var templateInfo = await _templateRepository.GetTemplateAsync(path).ConfigureAwait(false);
            if (templateInfo != null)
                view = await _viewEngine.CreateViewAsync(templateInfo).ConfigureAwait(false);

            if (view == null)
            {
                PageViewDefinition viewDefinition;
                if (_viewRepository.TryGetFromView(path, out viewDefinition))
                    return await Get(viewDefinition.Template, JObject.FromObject(viewDefinition));

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            object model;
            if (!string.IsNullOrEmpty(data))
                model = await _modelProvider.GetModelForTemplateAsync(templateInfo, data);
            else
                model = await _modelProvider.GetDefaultModelForTemplateAsync(templateInfo);

            return View(view, model);
        }

        private async Task<HttpResponseMessage> Get(string path, object data)
        {
            IView view = null;
            var templateInfo = await _templateRepository.GetTemplateAsync(path).ConfigureAwait(false);
            if (templateInfo != null)
                view = await _viewEngine.CreateViewAsync(templateInfo).ConfigureAwait(false);

            if (view == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            return View(view, data);
        }
    }

    public class TerrificViewDefinitionRepository
    {
        private readonly IFileSystem _fileSystem;
        private readonly PathInfo _viewPathInfo;

        public TerrificViewDefinitionRepository(IFileSystem fileSystem, ITerrificNetConfig configuration)
        {
            _fileSystem = fileSystem;
            _viewPathInfo = configuration.ViewPath;
        }

        public bool TryGetFromView(string path, out PageViewDefinition viewDefinition)
        {
            var fileName = _fileSystem.Path.ChangeExtension(_fileSystem.Path.Combine(_viewPathInfo, PathInfo.Create(path)), "html.json");
            if (_fileSystem.FileExists(fileName))
            {
                if (TryReadPageDefinition(out viewDefinition, fileName)) 
                    return true;
            }

            viewDefinition = null;
            return false;
        }

        public bool TryGetFromViewId(string id, out PageViewDefinition viewDefinition)
        {
            var fileId = PathInfo.Create(id);
            if (_fileSystem.FileExists(fileId))
            {
                if (TryReadPageDefinition(out viewDefinition, fileId))
                    return true;
            }

            viewDefinition = null;
            return false;
        }

        public async Task<bool> UpdateViewDefinitionForId(string id, PageViewDefinition viewDefinition)
        {
            var fileId = PathInfo.Create(id);
            if (!_fileSystem.FileExists(fileId)) 
                return false;

            await WritePageDefinition(viewDefinition, fileId);
            return true;
        }

        public IEnumerable<PageViewDefinition> GetAll()
        {
            foreach (var viewPath in _fileSystem.DirectoryGetFiles(_viewPathInfo, "html.json"))
            {
                PageViewDefinition viewDefinition;
                if (TryReadPageDefinition(out viewDefinition, viewPath))
                    yield return viewDefinition;
            }
        }

        private bool TryReadPageDefinition(out PageViewDefinition viewDefinition, PathInfo fileName)
        {
            using (var reader = new JsonTextReader(new StreamReader(_fileSystem.OpenRead(fileName))))
            {
                var jObj = JToken.ReadFrom(reader);
                viewDefinition = ViewDefinition.FromJObject<PageViewDefinition>(jObj);
                if (viewDefinition == null) 
                    return false;

                viewDefinition.Id = fileName.ToString();

                return true;
            }
        }

        private Task WritePageDefinition(PageViewDefinition viewDefinition, PathInfo fileName)
        {
            using (var stream = new StreamWriter(_fileSystem.OpenWrite(fileName)))
            {
                var value = JsonConvert.SerializeObject(viewDefinition);
                return stream.WriteAsync(value);
            }
        }
    }
}
