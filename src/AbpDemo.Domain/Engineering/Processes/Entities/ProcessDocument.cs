using System;
using Volo.Abp.Domain.Entities;

namespace AbpDemo.Engineering.Processes;

/// <summary>
/// 工艺文档子实体
/// </summary>
public class ProcessDocument : Entity<Guid>
{
    public Guid ProcessRouteId { get; private set; }
    public string DocumentName { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string FilePath { get; private set; }
    public long FileSize { get; private set; }
    public string Version { get; private set; }
    public Guid UploadedBy { get; private set; }
    public DateTime UploadedAt { get; private set; }

    protected ProcessDocument()
    {
    }

    public ProcessDocument(Guid id, Guid processRouteId, string documentName, DocumentType documentType,
        string filePath, long fileSize, string version, Guid uploadedBy)
    {
        Id = id;
        ProcessRouteId = processRouteId;
        SetDocumentName(documentName);
        DocumentType = documentType;
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        FileSize = fileSize;
        Version = version;
        UploadedBy = uploadedBy;
        UploadedAt = DateTime.UtcNow;
    }

    public void SetDocumentName(string documentName)
    {
        DocumentName = documentName ?? throw new ArgumentNullException(nameof(documentName));
    }
}

/// <summary>
/// 文档类型
/// </summary>
public enum DocumentType : byte
{
    SOP = 1, // 标准作业程序
    InspectionSpec = 2, // 检验规范
    Drawing = 3, // 图纸
    Other = 4 // 其他
}