
namespace Great2.Models.Interfaces
{
    public interface IFDLFile
    {
        string FileName { get; set; }
        string FilePath { get; }

        EFDLStatus EStatus { get; set; }
    }
}
