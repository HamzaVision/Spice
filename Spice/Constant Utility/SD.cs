using Spice.Models;
using System;
using System.Reflection.Metadata;

namespace Spice.Constant_Utility
{
    public static class SD
    {
        public const string DefaultPath = "default.png";
		public const string InKitchen = "\\Images\\InKitchen.png";
		public const string Completed = "\\Images\\completed.png";
		public const string OrderPlaced = "\\Images\\OrderPlaced.png";
		public const string ReadyForPickup = "\\Images\\ReadyForPickup.png";
		public const string ManagerRole = "Manager";
		public const string CustomerRole = "Customer";
		public const string FrontDeskRole = "FrontDesk";
		public const string KitchenRole = "Kitchen";
		public const string ssShoppingCart = "ssCartCount";
		public const string ssCouponCode = "ssCouponCode";


		public const string statusSubmitted = "statusSubmitted";
		public const string statusInProcess = "statusInProcess";
		public const string statusReady = "statusReady";
		public const string statusCompleted = "statusCompleted";
		public const string statusCancelled = "statusCancelled";

		public const string PaymentPending = "PaymentPending";
		public const string PaymentApproved = "PaymentApproved";
		public const string PaymentRejected = "PaymentRejected";




		public static string ConvertToRawHtml(string source)
		{
			char[] array = new char[source.Length];
			int arrayIndex = 0;
			bool inside = false;

			for (int i = 0; i < source.Length; i++)
			{
				char let = source[i];
				if (let == '<')
				{
					inside = true;
					continue;
				}
				if (let == '>')
				{
					inside = false;
					continue;
				}
				if (!inside)
				{
					array[arrayIndex] = let;
					arrayIndex++;
				}
			}
			return new string(array, 0, arrayIndex);
		}
		public static double DiscountPrice(Coupon couponfromdb, double originalprice)
		{
			if (couponfromdb == null)
			{
				return originalprice;
			}
			else {
				if (couponfromdb.MinimumAmount > originalprice)
				{
					return originalprice;
				}
				if (Convert.ToInt32(couponfromdb.CouponType) == (int)Coupon.ECouponType.Dollar)
				{
					//10 off 100
					return Math.Round(originalprice - couponfromdb.discount, 2);
				}
				if (Convert.ToInt32(couponfromdb.CouponType) == (int)Coupon.ECouponType.percent)
				{
					return Math.Round(originalprice - (originalprice * couponfromdb.discount/100), 2);
				}
			}
			return originalprice;
		}
	}
}
