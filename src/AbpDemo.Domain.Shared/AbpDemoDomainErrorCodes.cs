namespace AbpDemo;

public static class AbpDemoDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */

    // Workshop
    public const string WorkshopCodeRequired = "AbpDemo:Workshop:CodeRequired";
    public const string WorkshopNameRequired = "AbpDemo:Workshop:NameRequired";

    // WorkCenter
    public const string WorkCenterCodeRequired = "AbpDemo:WorkCenter:CodeRequired";
    public const string WorkCenterNameRequired = "AbpDemo:WorkCenter:NameRequired";
    public const string WorkCenterCapacityInvalid = "AbpDemo:WorkCenter:CapacityInvalid";
    public const string WorkCenterShiftCountInvalid = "AbpDemo:WorkCenter:ShiftCountInvalid";
}