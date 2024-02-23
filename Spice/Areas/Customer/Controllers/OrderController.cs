using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Spice.Data;
using Spice.Data.Migrations;
using Spice.Models;
using Spice.Models.ViewModels;
using System.Security.Claims;
using Spice.Constant_Utility;
using System.Drawing;
using System.Text;

namespace Spice.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class OrderController : Controller
	{
		private readonly ApplicationDbContext _db;
		private int pageSize = 2;
		public OrderController(ApplicationDbContext db)
		{
			_db = db;
		}
		public IActionResult Index()
		{

			return View();
		}
		[Authorize]
		public async Task<IActionResult> Confirm(int Id)
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			OrderDetailsViewModel OrderDetailsVM = new OrderDetailsViewModel()
			{
				OrderHeader = await _db.OrderHeader.Include(s => s.ApplicationUser).FirstOrDefaultAsync(s=>s.Id == Id && s.ApplicationUserId == claim.Value),
				OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == Id).ToListAsync()
			};
			return View(OrderDetailsVM);
		}
		[Authorize]
        public async Task<IActionResult> OrderHistory(int Page=1)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			OrderDetailsAndPagingViewModel order_detailsVM_list = new OrderDetailsAndPagingViewModel()
			{
				OrderDetailsList = new List<OrderDetailsViewModel>()
			};

			List<OrderHeader> orders = await _db.OrderHeader.Include(s => s.ApplicationUser).Where(s => s.ApplicationUserId == claim.Value).ToListAsync();
			
			foreach(OrderHeader order in orders)
			{
				OrderDetailsViewModel individual = new OrderDetailsViewModel()
				{
					OrderHeader = order,
					OrderDetails = await _db.OrderDetails.Where(s => s.OrderId == order.Id).ToListAsync()
				};
				order_detailsVM_list.OrderDetailsList.Add(individual);
			}
			var count = order_detailsVM_list.OrderDetailsList.Count;
			order_detailsVM_list.OrderDetailsList = order_detailsVM_list.OrderDetailsList.OrderByDescending(p => p.OrderHeader.Id).Skip((Page = -1) * pageSize).Take(pageSize).ToList();

			order_detailsVM_list.pagingObj = new PagingInfo()
			{
				CurrentPage = Page,
				itemsPerPage = pageSize,
				totalItems = count,
				urlParam = "/Customer/Order/OrderHistory?Page=:"
			};



			return View(order_detailsVM_list);
        }

		[Authorize]
		public async Task<IActionResult> ManageOrder()
		{
			
			List<OrderDetailsViewModel> order_detailsVM_list = new List<OrderDetailsViewModel>();
			List<OrderHeader> orders = await _db.OrderHeader.Where(s => s.Status == SD.statusSubmitted || s.Status == SD.statusInProcess)
				.OrderByDescending(o=>o.PickUpTime).ToListAsync();

			foreach (OrderHeader order in orders)
			{
				OrderDetailsViewModel individual = new OrderDetailsViewModel()
				{
					OrderHeader = order,
					OrderDetails = await _db.OrderDetails.Where(s => s.OrderId == order.Id).ToListAsync()
				};
				order_detailsVM_list.Add(individual);
			}
            return View(order_detailsVM_list);
		}


		public async Task<IActionResult> GetOrderDetails(int id)
		{

			OrderDetailsViewModel individual = new OrderDetailsViewModel()
			{
				OrderHeader = await _db.OrderHeader.FirstOrDefaultAsync(s=>s.Id == id),
				OrderDetails = await _db.OrderDetails.Where(s=>s.OrderId == id).ToListAsync()
			};
			individual.OrderHeader.ApplicationUser = await _db.ApplicationUser.FirstOrDefaultAsync(s => s.Id == individual.OrderHeader.ApplicationUserId);
			return PartialView("_IndividualOrderDetails", individual);
		}

		public async Task<IActionResult> GetOrderStatus(int id)
		{
			var order = await _db.OrderHeader.FirstOrDefaultAsync(s => s.Id == id);
			var image = "";

			if (order.Status == SD.statusSubmitted)
			{
				image = SD.OrderPlaced;
			}
			else if (order.Status == SD.statusInProcess)
			{
				image = SD.statusInProcess;
			}
			else if (order.Status == SD.statusReady)
			{
				image = SD.ReadyForPickup;
			}
			else
			{
				image = SD.Completed;
			}
			return PartialView("_IndividualStatusPartialView", image);
		}

		[Authorize(Roles=SD.ManagerRole +","+SD.KitchenRole)]
        public async Task<IActionResult> OrderPrepared(int orderId)
        {

			OrderHeader order = await _db.OrderHeader.FirstOrDefaultAsync(s=>s.Id == orderId);
			order.Status = SD.statusInProcess;
			await _db.SaveChangesAsync();
            return RedirectToAction("ManageOrder","Order");
        }

        [Authorize(Roles = SD.ManagerRole + "," + SD.KitchenRole)]
        public async Task<IActionResult> OrderReady(int orderId)
        {

            OrderHeader order = await _db.OrderHeader.FirstOrDefaultAsync(s => s.Id == orderId);
            order.Status = SD.statusReady;
            await _db.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }
        [Authorize(Roles = SD.ManagerRole + "," + SD.KitchenRole)]
        public async Task<IActionResult> CancelOrder(int orderId)
        {

            OrderHeader order = await _db.OrderHeader.FirstOrDefaultAsync(s => s.Id == orderId);
            order.Status = SD.statusCancelled;
            await _db.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }

        public async Task<IActionResult> OrderPickUp(int Page = 1)
        {
            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsAndPagingViewModel order_detailsVM_list = new OrderDetailsAndPagingViewModel()
            {
                OrderDetailsList = new List<OrderDetailsViewModel>()
            };
			StringBuilder url = new StringBuilder();
			url.Append("/Customer/Order/OrderPickUp?Page=:");
            List<OrderHeader> orders = await _db.OrderHeader.Include(s => s.ApplicationUser).Where(s => s.Status == SD.statusReady).ToListAsync();

            foreach (OrderHeader order in orders)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel()
                {
                    OrderHeader = order,
                    OrderDetails = await _db.OrderDetails.Where(s => s.OrderId == order.Id).ToListAsync()
                };
                order_detailsVM_list.OrderDetailsList.Add(individual);
            }
            var count = order_detailsVM_list.OrderDetailsList.Count;
            order_detailsVM_list.OrderDetailsList = order_detailsVM_list.OrderDetailsList.OrderByDescending(p => p.OrderHeader.Id).Skip((Page = -1) * pageSize).Take(pageSize).ToList();

            order_detailsVM_list.pagingObj = new PagingInfo()
            {
                CurrentPage = Page,
                itemsPerPage = pageSize,
                totalItems = count,
                urlParam = url.ToString()
            };



            return View(order_detailsVM_list);
        }

		[Authorize(Roles = SD.FrontDeskRole + "," + SD.ManagerRole)]
		public async Task<IActionResult> OrderPickUpPost(int orderId)
		{
            OrderHeader order = await _db.OrderHeader.FirstOrDefaultAsync(s => s.Id == orderId);
            order.Status = SD.statusCompleted;
            await _db.SaveChangesAsync();
            return RedirectToAction("OrderPickUp", "Order");
        }
    }
}
