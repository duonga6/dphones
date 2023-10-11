using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace App.Services
{

    public class SidebarAdminService
    {
        public readonly IUrlHelper urlHelper;

        private readonly ILogger<SidebarAdminService> _logger;

        public List<SidebarItem> Items { set; get; } = new();

        public SidebarAdminService(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, ILogger<SidebarAdminService> logger)
        {
            _logger = logger;

            if (actionContextAccessor.ActionContext == null)
                throw new Exception("ActionContext null. SidebarAdminService.cs");

            urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);

            Items.Add(new SidebarItem()
            {
                Title = "Trang chính",
                Type = SidebarItemType.Heading
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Dashboard",
                Action = "Index",
                Controller = "AdminCP",
                Area = "AdminCP",
                AwIcon = "fa-solid fa-gauge-high"
            });

            Items.Add(new SidebarItem()
            {
                Title = "Quản lý",
                Type = SidebarItemType.Heading
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Quản lý người dùng",
                AwIcon = "fa-solid fa-users",
                CollapseID = "user-manager",
                Items = new List<SidebarItem>() {
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Quản lý User",
                        Action = "Index",
                        Controller = "User",
                        Area = "Identity"
                    },
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Quản lý Role",
                        Action = "Index",
                        Controller = "Roles",
                        Area = "Identity"
                    }
                }
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Quản lý sản phẩm",
                AwIcon = "fa-solid fa-box-archive",
                CollapseID = "product-manager",
                Items = new List<SidebarItem>() {
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Quản lý sản phẩm",
                        Action = "Index",
                        Controller = "Product",
                        Area = "Products"
                    },
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Quản lý hãng",
                        Action = "Index",
                        Controller = "Brand",
                        Area = "Products"
                    },
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Quản lý danh mục",
                        Action = "Index",
                        Controller = "Category",
                        Area = "Products"
                    }
                }
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Quản lý đơn hàng",
                Action = "Index",
                Controller = "Order",
                Area = "Products",
                AwIcon = "fa-solid fa-cart-shopping"
            });
        }

        public string RenderHtml(string controller, string area)
        {
            SetActive(controller, area);
            var html = new StringBuilder();

            foreach (var item in Items)
            {
                html.Append(item.RenderHtml(urlHelper));
            }

            return html.ToString();
        }

        public void SetActive(string Controller, string Area)
        {
            if (Items == null) return;

            foreach (var item in Items)
            {
                if (item.Controller == Controller && item.Area == Area)
                {
                    item.IsActive = true;
                    return;
                }
                else
                {
                    if (item.Items != null)
                    {
                        foreach (var subitem in item.Items)
                        {
                            if (subitem.Controller == Controller && subitem.Area == Area)
                            {
                                subitem.IsActive = true;
                                item.IsActive = true;
                                return;
                            }
                        }
                    }
                }

            }
        }
    }
}