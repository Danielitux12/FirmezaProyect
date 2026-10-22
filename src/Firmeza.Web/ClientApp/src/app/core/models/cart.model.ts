import { ProductDto } from '../../features/products/models/product.model';

export interface CartItem {
  product: ProductDto;
  quantity: number;
}
