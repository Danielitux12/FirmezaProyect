export interface ProductDto {
  id: string;
  name: string;
  description: string;
  price: number;
  stock: number;
  isActive: boolean;
}

export interface CreateProductDto {
  name: string;
  description: string;
  price: number;
  stock: number;
}
