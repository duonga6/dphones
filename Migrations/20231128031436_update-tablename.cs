using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
    /// <inheritdoc />
    public partial class updatetablename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colors_Product_ProductId",
                table: "Colors");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Product_ProductId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Brand_BrandId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategory_Category_CategoryId",
                table: "ProductCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategory_Product_ProductId",
                table: "ProductCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductDiscount_Discounts_DiscountId",
                table: "ProductDiscount");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductDiscount_Product_ProductId",
                table: "ProductDiscount");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPhoto_Product_ProductId",
                table: "ProductPhoto");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Product_ProductId",
                table: "Reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductPhoto",
                table: "ProductPhoto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductDiscount",
                table: "ProductDiscount");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductCategory",
                table: "ProductCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Category",
                table: "Category");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Brand",
                table: "Brand");

            migrationBuilder.RenameTable(
                name: "ProductPhoto",
                newName: "ProductPhotos");

            migrationBuilder.RenameTable(
                name: "ProductDiscount",
                newName: "ProductDiscounts");

            migrationBuilder.RenameTable(
                name: "ProductCategory",
                newName: "ProductCategories");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "Category",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "Brand",
                newName: "Brands");

            migrationBuilder.RenameIndex(
                name: "IX_ProductPhoto_ProductId",
                table: "ProductPhotos",
                newName: "IX_ProductPhotos_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductDiscount_ProductId",
                table: "ProductDiscounts",
                newName: "IX_ProductDiscounts_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductCategory_ProductId",
                table: "ProductCategories",
                newName: "IX_ProductCategories_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_Slug",
                table: "Products",
                newName: "IX_Products_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_Product_BrandId",
                table: "Products",
                newName: "IX_Products_BrandId");

            migrationBuilder.RenameIndex(
                name: "IX_Category_Slug",
                table: "Categories",
                newName: "IX_Categories_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_Brand_Slug",
                table: "Brands",
                newName: "IX_Brands_Slug");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductPhotos",
                table: "ProductPhotos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductDiscounts",
                table: "ProductDiscounts",
                columns: new[] { "DiscountId", "ProductId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductCategories",
                table: "ProductCategories",
                columns: new[] { "CategoryId", "ProductId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Brands",
                table: "Brands",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Colors_Products_ProductId",
                table: "Colors",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_Categories_CategoryId",
                table: "ProductCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_Products_ProductId",
                table: "ProductCategories",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductDiscounts_Discounts_DiscountId",
                table: "ProductDiscounts",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductDiscounts_Products_ProductId",
                table: "ProductDiscounts",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPhotos_Products_ProductId",
                table: "ProductPhotos",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Colors_Products_ProductId",
                table: "Colors");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_Categories_CategoryId",
                table: "ProductCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_Products_ProductId",
                table: "ProductCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductDiscounts_Discounts_DiscountId",
                table: "ProductDiscounts");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductDiscounts_Products_ProductId",
                table: "ProductDiscounts");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPhotos_Products_ProductId",
                table: "ProductPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Products_ProductId",
                table: "Reviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductPhotos",
                table: "ProductPhotos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductDiscounts",
                table: "ProductDiscounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductCategories",
                table: "ProductCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Brands",
                table: "Brands");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameTable(
                name: "ProductPhotos",
                newName: "ProductPhoto");

            migrationBuilder.RenameTable(
                name: "ProductDiscounts",
                newName: "ProductDiscount");

            migrationBuilder.RenameTable(
                name: "ProductCategories",
                newName: "ProductCategory");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Category");

            migrationBuilder.RenameTable(
                name: "Brands",
                newName: "Brand");

            migrationBuilder.RenameIndex(
                name: "IX_Products_Slug",
                table: "Product",
                newName: "IX_Product_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_Products_BrandId",
                table: "Product",
                newName: "IX_Product_BrandId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductPhotos_ProductId",
                table: "ProductPhoto",
                newName: "IX_ProductPhoto_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductDiscounts_ProductId",
                table: "ProductDiscount",
                newName: "IX_ProductDiscount_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductCategories_ProductId",
                table: "ProductCategory",
                newName: "IX_ProductCategory_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_Slug",
                table: "Category",
                newName: "IX_Category_Slug");

            migrationBuilder.RenameIndex(
                name: "IX_Brands_Slug",
                table: "Brand",
                newName: "IX_Brand_Slug");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductPhoto",
                table: "ProductPhoto",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductDiscount",
                table: "ProductDiscount",
                columns: new[] { "DiscountId", "ProductId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductCategory",
                table: "ProductCategory",
                columns: new[] { "CategoryId", "ProductId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Category",
                table: "Category",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Brand",
                table: "Brand",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Colors_Product_ProductId",
                table: "Colors",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Product_ProductId",
                table: "OrderDetails",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Brand_BrandId",
                table: "Product",
                column: "BrandId",
                principalTable: "Brand",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategory_Category_CategoryId",
                table: "ProductCategory",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategory_Product_ProductId",
                table: "ProductCategory",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductDiscount_Discounts_DiscountId",
                table: "ProductDiscount",
                column: "DiscountId",
                principalTable: "Discounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductDiscount_Product_ProductId",
                table: "ProductDiscount",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPhoto_Product_ProductId",
                table: "ProductPhoto",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Product_ProductId",
                table: "Reviews",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
