namespace SimpleChat.Models
{
    /// <summary>
    /// ����������� ������� ����� ���������� �������������� ������������.
    /// </summary>
    public abstract class AuthenticationResultBase : IAuthResult
    {
        public string Type { get; } = "auth_check";

        /// <summary>
        /// ����������, ����������� ������������ ��� ���.
        /// </summary>
        public abstract bool IsAuthenticated { get; }
    }
}
