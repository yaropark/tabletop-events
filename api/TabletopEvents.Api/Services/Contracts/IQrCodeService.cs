namespace TabletopEvents.Api.Services.Contracts;

public interface IQrCodeService
{
    /// <summary>Encodes the text as a QR code PNG image.</summary>
    byte[] CreatePng(string text);
}
