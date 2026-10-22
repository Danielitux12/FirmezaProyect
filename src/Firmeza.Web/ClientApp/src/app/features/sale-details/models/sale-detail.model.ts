export interface SaleDetailDto {
  id: string;
  productId: string;
  productName?: string | null;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface CreateSaleDetailDto {
  productId: string;
  quantity: number;
  unitPrice: number;
}
