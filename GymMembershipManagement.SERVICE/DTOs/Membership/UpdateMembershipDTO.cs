namespace GymMembershipManagement.SERVICE.DTOs.Membership
{
    public class UpdateMembershipDTO
    {
        public int MembershipTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
    }
}
