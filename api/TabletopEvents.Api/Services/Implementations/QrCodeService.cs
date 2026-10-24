using QRCoder;
using TabletopEvents.Api.Services.Contracts;

namespace TabletopEvents.Api.Services.Implementations;

public sealed class QrCodeService : IQrCodeService
{
    public byte[] CreatePng(string text) => PngByteQRCodeHelper.GetQRCode(text, QRCodeGenerator.ECCLevel.Q, 10);
}
