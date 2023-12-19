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
                Title = "Database",
                Action = "Index",
                Controller = "DbManager",
                Area = "Database",
                AwIcon = "fa-solid fa-database"
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Tài khoản",
                AwIcon = "fa-solid fa-users",
                CollapseID = "user-manager",
                Items = new List<SidebarItem>() {
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Quản lý KH",
                        Action = "Index",
                        Controller = "User",
                        Area = "Identity"
                    },
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Quản lý quyền",
                        Action = "Index",
                        Controller = "Roles",
                        Area = "Identity"
                    }
                }
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Sản phẩm",
                AwIcon = "fa-solid fa-box-archive",
                CollapseID = "product-manager",
                Items = new List<SidebarItem>() {
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Tất cả sản phẩm",
                        Action = "Index",
                        Controller = "Product",
                        Area = "Products"
                    },
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Hãng sản xuất",
                        Action = "Index",
                        Controller = "Brand",
                        Area = "Products"
                    },
                    new() {
                        Type = SidebarItemType.NavItem,
                        Title = "Danh mục",
                        Action = "Index",
                        Controller = "Category",
                        Area = "Products"
                    }
                }
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Đơn hàng",
                Action = "Index",
                Controller = "Order",
                Area = "Products",
                AwIcon = "fa-solid fa-cart-shopping"
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "KM - Giảm giá",
                Action = "Index",
                Controller = "Discount",
                Area = "Products",
                AwIcon = "fa-solid fa-tags"
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Tin tức",
                Action = "Index",
                Controller = "Post",
                Area = "Posts",
                AwIcon = "fa-solid fa-newspaper"
            });

            Items.Add(new SidebarItem()
            {
                Title = "Tiện ích",
                Type = SidebarItemType.Heading
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Gửi mail",
                Action = "SendMail",
                Controller = "AdminCP",
                Area = "AdminCP",
                AwIcon = "fa-solid fa-envelope"
            });

            Items.Add(new SidebarItem()
            {
                Title = "Khác",
                Type = SidebarItemType.Heading
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Tin nhắn",
                Action = "Index",
                Controller = "Message",
                Area = "AdminCP",
                AwIcon = "fa-solid fa-message"
            });

            Items.Add(new SidebarItem()
            {
                Type = SidebarItemType.NavItem,
                Title = "Liên hệ",
                Action = "Index",
                Controller = "Contact",
                Area = "Contacts",
                AwIcon = "fa-solid fa-address-card"
            });
        }

        public string RenderHtml(string controller, string area, string action = "")
        {
            SetActive(controller, area, action);

            var html = new StringBuilder();

            foreach (var item in Items)
            {
                html.Append(item.RenderHtml(urlHelper));
            }

            return html.ToString();
        }

        public void SetActive(string Controller, string Area, string Action)
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