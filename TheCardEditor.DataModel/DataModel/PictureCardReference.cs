namespace TheCardEditor.DataModel.DataModel;

public partial class PictureCardReference
{
    public PictureCardReference()
    {
    }

    public int Id { get; set; }
    public int PictureFk { get; set; }
    public int CardFk { get; set; }
    public virtual Card CardFkNavigation { get; set; } = null!;
    public virtual Picture PictureFkNavigation { get; set; } = null!;
}
