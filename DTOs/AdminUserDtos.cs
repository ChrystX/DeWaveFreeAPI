namespace DeWaveFreeAPI.DTOs
{
    public class AdminUserDtos
    {
    }

    public class CreateUserDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
    }

    public class UpdateUserDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
    }

    public class ChangeRoleDto
    {
        public int RoleId { get; set; }
    }
}
