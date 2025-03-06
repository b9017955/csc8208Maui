
    public class AccountDTO
    {
        public enum AccountType {
            USER,ADMIN
        }
        public int id { get; set; }
        
        public string firstName { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public AccountType type { get; set; }
        public string appPublicKey { get; set; }
    }

