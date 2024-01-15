namespace Equinor.ProCoSys.Common.TemplateTransforming;

public interface ITemplateTransformer
{
    string Transform(string template, object transformationContext);
}
