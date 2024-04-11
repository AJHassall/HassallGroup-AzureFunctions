namespace HassallGroup.Function
{
    public class ReCaptchaValidationService{

        private const string RECAPTCHA_VERIFICATION_URL = "https://www.google.com/recaptcha/api/siteverify";
        public static async Task<bool> VerifyReCaptchaAsync(string token)
        {

            string recaptchaKey = Environment.GetEnvironmentVariable("RECAPTCHA_KEY");

            using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("secret", recaptchaKey),  // Store your secret in App Settings
                    new KeyValuePair<string, string>("response", token)
                });

                var verificationResponse = await httpClient.PostAsync(RECAPTCHA_VERIFICATION_URL, content);
                var verificationResult = await verificationResponse.Content.ReadFromJsonAsync<ReCaptchaVerificationResult>();

                return verificationResult.success;
            }
        }
    }
        public class ReCaptchaVerificationResult
        {
            public bool success { get; set; }
            // Other properties may be included in the response
        }
}