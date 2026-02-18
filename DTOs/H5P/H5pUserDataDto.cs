namespace DeWaveFreeAPI.DTOs.H5P
{
    public record H5pUserDataDto(
        string SubContentId,
        string DataId,
        string? Data,
        bool Preload,
        bool Invalidate
    );

    public record H5pSetUserDataRequest(
        string SubContentId,
        string DataId,
        string? Data,
        bool Preload = false,
        bool Invalidate = false
    );

    public record H5pFinishedRequest(
        int ContentId,
        decimal Score,
        decimal MaxScore,
        long Opened,   // Unix timestamp
        long Finished  // Unix timestamp
    );
}
