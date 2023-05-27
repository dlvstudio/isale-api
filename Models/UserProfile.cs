using System;

public class UserProfile {
    public string UserId { get; set; }

    public string Email { get; set; }

    public string DisplayName { get; set; }

    public string AvatarUrl { get; set; }

    public string FacebookId { get; set; }

    public string GoogleId { get; set; }

    public bool? Gender { get; set; }

    public bool IsEmailMethod { get; set; }

    public string PhoneNumber { get; set; }

    public DateTime? Birthday { get; set; }

    public int Rank { get; set; }

    public string GenderString { 
        get {
            if (!Gender.HasValue) {
                return string.Empty;
            }
            if (Gender.Value) {
                return "male";
            }
            return "female";
        } 
    }
}