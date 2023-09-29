using System.Text;
using Microsoft.AspNetCore.Mvc;

public enum SidebarItemType
    {
        Heading,
        NavItem
    }

    public class SidebarItem
    {
        public string? Title { set; get; }
        public bool IsActive { set; get; }
        public required SidebarItemType Type { set; get; }
        public string? Action { set; get; }
        public string? Controller { set; get; }
        public string? Area { set; get; }
        public string? AwIcon { set; get; }
        public List<SidebarItem>? Items { set; get; }
        public string? CollapseID { set; get; }

        public string? GetLink(IUrlHelper urlHelper) => urlHelper.Action(Action, Controller, new { area = Area });

        public string RenderHtml(IUrlHelper urlHelper)
        {
            var html = new StringBuilder();

            if (Type == SidebarItemType.Heading)
            {
                html.Append($@"
                    <div class=""sb-sidenav-menu-heading"">{Title}</div>
                ");
            }
            else if (Type == SidebarItemType.NavItem)
            {
                if (Items == null)
                {
                    var url = GetLink(urlHelper);
                    var active = IsActive ? "active" : "";
                    html.Append($@"
                        <a class=""nav-link {active}"" href=""{url}"">
                            <div class=""sb-nav-link-icon""><i class=""{AwIcon}""></i></div>
                            {Title}
                        </a>
                    ");
                }
                else
                {
                    var listItem = new StringBuilder();
                    foreach (var item in Items)
                    {
                        var itemActive = item.IsActive ? "active" : "";
                        var itemUrl = item.GetLink(urlHelper);
                        listItem.Append($@"
                            <a class=""nav-link {itemActive}"" href=""{itemUrl}"">{item.Title}</a>
                        ");
                    }

                    var active = IsActive ? "active" : "";
                    var show = IsActive ? "show" : "";
                    html.Append($@"
                        <a class=""nav-link collapsed {active}"" href=""#"" data-bs-toggle=""collapse""
                            data-bs-target=""#{CollapseID}"" aria-expanded=""false"" aria-controls=""{CollapseID}"">
                            <div class=""sb-nav-link-icon""><i class=""{AwIcon}""></i></div>
                            {Title}
                            <div class=""sb-sidenav-collapse-arrow""><i class=""fas fa-angle-down""></i></div>
                        </a>
                        
                        <div class=""collapse {show}"" id=""{CollapseID}"" aria-labelledby=""headingOne""
                            data-bs-parent=""#sidenavAccordion"">
                            <nav class=""sb-sidenav-menu-nested nav"">
                                {listItem.ToString()}
                            </nav>
                        </div>
                    ");
                }
            }

            return html.ToString();
        }
    }
