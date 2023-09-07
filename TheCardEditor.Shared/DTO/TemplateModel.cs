using System.ComponentModel.DataAnnotations;
using TheCardEditor.DataModel.DataModel;

namespace TheCardEditor.DataModel.DTO;

public class TemplateModel
{
    public TemplateModel()
    { }

    public TemplateModel(Template template)
    {
        Id = template.Id;
        Name = template.Name;
        Data = template.Data;
        CardSetFk = template.CardSetFk;
    }

    public Template GetDataModel()
    {
        return new Template()
        {
            Name = Name,
            CardSetFk = CardSetFk,
            Data = Data,
            Id = Id
        };
    }

    public long Id { get; set; }

    [Required]
    [MinLength(3, ErrorMessage = "Template name must be atleast 3 letters long")]
    public string Name { get; set; } = "";

    public string Data { get; set; } = null!;
    public long CardSetFk { get; set; }
}
