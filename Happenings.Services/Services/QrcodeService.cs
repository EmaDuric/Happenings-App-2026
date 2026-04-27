using QRCoder;

namespace Happenings.Services.Services;

public class QrCodeService
{
    public string GenerateQRCode(string text)
    {
        using var qrGenerator = new QRCodeGenerator();

        var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

        var qrCode = new PngByteQRCode(qrCodeData);

        var bytes = qrCode.GetGraphic(20);

        return Convert.ToBase64String(bytes);
    }
}