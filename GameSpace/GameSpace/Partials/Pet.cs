using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models;

public partial class Pet
{
    /// <summary>
    /// 名稱別名（向後相容）
    /// </summary>
    [NotMapped]
    public string Name
    {
        get => PetName;
        set => PetName = value;
    }

    /// <summary>
    /// 背景別名（向後相容）
    /// </summary>
    [NotMapped]
    public string Background
    {
        get => BackgroundColor;
        set => BackgroundColor = value;
    }

    /// <summary>
    /// 是否啟用（預設為 true）
    /// </summary>
    [NotMapped]
    public bool IsActive { get; set; } = true;
}
