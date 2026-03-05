namespace gsa_api
{
    public class Helper
    {
        public static IResult errMessage(string msg, int code = 422)
        {
            return Results.Json(new { message = msg }, statusCode: code);
        }
    }

    public class PurchaseData
    {
        public string PaymentMethod { get; set; } = null!;
        public string CouponCode { get; set; } = null!;
    }
}
