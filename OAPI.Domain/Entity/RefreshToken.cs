using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Domain.Entity
{
	public class RefreshToken
	{
		public Guid RefreshTokenId { get; private set; }
		public string TokenHash { get; private set; }
		public string UserEmail { get; private set; }
		public DateTime ExpiryDate { get; private set; }
		public bool IsRevoked { get; private set; }
		public DateTime CreatedAt { get; private set; }
		public string? ReplacedByTokenHash { get; private set; }

		public RefreshToken(string tokenHash, string userEmail)
		{
			RefreshTokenId = Guid.NewGuid();
			TokenHash = tokenHash;
			UserEmail = userEmail;
			ExpiryDate = DateTime.UtcNow.AddDays(7); // Set expiry to 7 days from now
			IsRevoked = false;
			CreatedAt = DateTime.UtcNow;
		}

		private RefreshToken() { }

		public void Revoke(string replacedByTokenHash = null)
		{
			IsRevoked = true;
			ReplacedByTokenHash = replacedByTokenHash;
		}
	}
}
