using System.ComponentModel.DataAnnotations;
using TheCardEditor.DataModel.DataModel;

namespace TheCardEditor.Shared.DTO;

public class FontModel
{
    public FontModel()
    { }

    public FontModel(Font font)
    {
        Id = font.Id;
        Base64Data = font.Base64Data;
        Name = font.Name;
    }

    public long Id { get; set; }

    [Required]
    public string Base64Data { get; set; } = null!;

    [RegularExpression("([A-Z]|[a-z]|[1-9]){3,}", ErrorMessage = "Only numbers and letters are allowed as name")]
    public string Name { get; set; } = null!;

    public Font GetDataModel()
    {
        return new Font()
        {
            Base64Data = Base64Data,
            Name = Name
        };
    }
}
