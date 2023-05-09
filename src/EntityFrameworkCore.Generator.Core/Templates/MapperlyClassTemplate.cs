using EntityFrameworkCore.Generator.Extensions;
using EntityFrameworkCore.Generator.Metadata.Generation;
using EntityFrameworkCore.Generator.Options;

namespace EntityFrameworkCore.Generator.Templates;

public class MapperlyClassTemplate : CodeTemplateBase
{
    private readonly Entity _entity;

    public MapperlyClassTemplate(Entity entity, GeneratorOptions options) : base(options)
    {
        _entity = entity;
    }

    public override string WriteCode()
    {
        CodeBuilder.Clear();

        CodeBuilder.AppendLine("using Riok.Mapperly.Abstractions;");

        //var imports = new SortedSet<string>();
        //imports.Add(_entity.EntityNamespace);

        //foreach (var model in _entity.Models)
        //    imports.Add(model.ModelNamespace);

        //foreach (var import in imports)
        //    if (_entity.MapperNamespace != import)
        //        CodeBuilder.AppendLine($"using {import};");

        CodeBuilder.AppendLine();

        CodeBuilder.Append($"namespace {_entity.MapperNamespace}");

        if (Options.Data.Context.FileScopedNamespace)
        {
            CodeBuilder.AppendLine(";");
            GenerateClass();
        }
        else
        {
            CodeBuilder.AppendLine();
            CodeBuilder.AppendLine("{");

            using (CodeBuilder.Indent())
            {
                GenerateClass();
            }

            CodeBuilder.AppendLine("}");
        }

        return CodeBuilder.ToString();
    }

    private void GenerateClass()
    {
        var entityClass = _entity.EntityClass.ToSafeName();
        var mapperClass = _entity.MapperClass.ToSafeName();

        if (Options.Model.Mapper.Document)
        {
            CodeBuilder.AppendLine("/// <summary>");
            CodeBuilder.AppendLine($"/// Mapper class for entity <see cref=\"{entityClass}\"/> .");
            CodeBuilder.AppendLine("/// </summary>");
        }

        CodeBuilder.AppendLine("[Mapper]");
        CodeBuilder.AppendLine($"public partial class {mapperClass}");

        if (_entity.MapperBaseClass.HasValue())
        {
            var mapperBaseClass = _entity.MapperBaseClass.ToSafeName();
            using (CodeBuilder.Indent())
                CodeBuilder.AppendLine($": {mapperBaseClass}");
        }

        CodeBuilder.AppendLine("{");

        using (CodeBuilder.Indent())
        {
            GenerateMapperPartials();
        }

        CodeBuilder.AppendLine("}");
    }

    private void GenerateMapperPartials()
    {
        var mapperClass = _entity.MapperClass.ToSafeName();

        var entityClass = _entity.EntityClass.ToSafeName();
        var entityFullName = $"{_entity.EntityNamespace}.{entityClass}";
        string readFullName = null;
        string updateFullName = null;

        //using (CodeBuilder.Indent())
        //{
        foreach (var model in _entity.Models)
        {
            //CodeBuilder.AppendLine($"public {mapperClass}()");
            //CodeBuilder.AppendLine("{");
          var modelClass = model.ModelClass.ToSafeName();
            var modelFullName = $"{model.ModelNamespace}.{modelClass}";

            switch (model.ModelType)
            {
                case ModelType.Read:
                    readFullName = modelFullName;

                    if (Options.Model.Mapper.Document)
                    {
                        CodeBuilder.AppendLine("/// <summary>");
                        CodeBuilder.AppendLine($"/// Maps <see cref=\"{entityFullName}\"/> to <see cref=\"{modelFullName}\"/>.");
                        CodeBuilder.AppendLine("/// </summary>");
                    }
                    CodeBuilder.AppendLine($"public partial {modelFullName} ToQuery({entityFullName} entity);").AppendLine();

                    if (Options.Model.Mapper.Document)
                    {
                        CodeBuilder.AppendLine("/// <summary>");
                        CodeBuilder.AppendLine($"/// Maps <see cref=\"{entityFullName}\"/> to <see cref=\"{modelFullName}\"/>.");
                        CodeBuilder.AppendLine("/// </summary>");
                    }
                    CodeBuilder.AppendLine($"public partial IEnumerable<{modelFullName}> ToQuery(IEnumerable<{entityFullName}> entity);").AppendLine();
                    break;

                case ModelType.Create:
                    updateFullName = modelFullName;

                    if (Options.Model.Mapper.Document)
                    {
                        CodeBuilder.AppendLine("/// <summary>");
                        CodeBuilder.AppendLine($"/// Maps <see cref=\"{modelFullName}\"/> to <see cref=\"{entityFullName}\"/>.");
                        CodeBuilder.AppendLine("/// </summary>");
                    }
                    CodeBuilder.AppendLine($"public partial {entityFullName} ToEntity({modelFullName} command);").AppendLine();

                    if (Options.Model.Mapper.Document)
                    {
                        CodeBuilder.AppendLine("/// <summary>");
                        CodeBuilder.AppendLine($"/// Maps <see cref=\"{modelFullName}\"/> to <see cref=\"{entityFullName}\"/>.");
                        CodeBuilder.AppendLine("/// </summary>");
                    }
                    CodeBuilder.AppendLine($"public partial IEnumerable<{entityFullName}> ToEntity(IEnumerable<{modelFullName}> command);").AppendLine();
                    break;

                case ModelType.Update:
                    break;
            }

            // include support for coping read model to update model
            if (readFullName.HasValue() && updateFullName.HasValue())
            {
                updateFullName = modelFullName;
                if (Options.Model.Mapper.Document)
                {
                    CodeBuilder.AppendLine("/// <summary>");
                    CodeBuilder.AppendLine($"/// Maps <see cref=\"{readFullName}\"/> to <see cref=\"{updateFullName}\"/>.");
                    CodeBuilder.AppendLine("/// </summary>");
                }
                CodeBuilder.AppendLine($"public partial {updateFullName} ToCommand({readFullName} query);").AppendLine();

                if (Options.Model.Mapper.Document)
                {
                    CodeBuilder.AppendLine("/// <summary>");
                    CodeBuilder.AppendLine($"/// Maps <see cref=\"{readFullName}\"/> to <see cref=\"{updateFullName}\"/>.");
                    CodeBuilder.AppendLine("/// </summary>");
                }
                CodeBuilder.AppendLine($"public partial IEnumerable<{updateFullName}> ToCommand(IEnumerable<{readFullName}> query);").AppendLine();
                readFullName = null;
                updateFullName = null;
            }
        }
        //}
        //CodeBuilder.AppendLine("}");
        //CodeBuilder.AppendLine();
    }
}
