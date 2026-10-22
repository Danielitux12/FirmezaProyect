import { Injectable, computed, signal } from '@angular/core';
import { ProductDto } from '../../features/products/models/product.model';
import { CartItem } from '../models/cart.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private readonly _items = signal<CartItem[]>([]);
  readonly items = this._items.asReadonly();

  readonly totalItems = computed(() =>
    this._items().reduce((sum, item) => sum + item.quantity, 0)
  );

  readonly totalAmount = computed(() =>
    this._items().reduce((sum, item) => sum + (item.product.price * item.quantity), 0)
  );

  /**
   * Agrega un producto al carrito respetando el stock disponible.
   * Retorna true si se pudo agregar, false si no hay suficiente stock.
   */
  addItem(product: ProductDto, quantity: number = 1): boolean {
    if (!product || product.stock <= 0 || quantity <= 0) {
      return false;
    }

    const current = this._items();
    const existingIndex = current.findIndex(i => i.product.id === product.id);

    if (existingIndex > -1) {
      const existingItem = current[existingIndex];
      const newQuantity = existingItem.quantity + quantity;

      if (newQuantity > product.stock) {
        return false;
      }

      const updated = [...current];
      updated[existingIndex] = { ...existingItem, quantity: newQuantity };
      this._items.set(updated);
      return true;
    } else {
      if (quantity > product.stock) {
        return false;
      }

      this._items.set([...current, { product, quantity }]);
      return true;
    }
  }

  /**
   * Modifica la cantidad de un ítem en el carrito validando el stock.
   */
  updateQuantity(productId: string, quantity: number): boolean {
    const current = this._items();
    const item = current.find(i => i.product.id === productId);
    if (!item) return false;

    if (quantity <= 0) {
      this.removeItem(productId);
      return true;
    }

    if (quantity > item.product.stock) {
      return false;
    }

    const updated = current.map(i =>
      i.product.id === productId ? { ...i, quantity } : i
    );
    this._items.set(updated);
    return true;
  }

  /**
   * Quita un producto del carrito.
   */
  removeItem(productId: string): void {
    this._items.set(this._items().filter(i => i.product.id !== productId));
  }

  /**
   * Vacía todos los ítems del carrito.
   */
  clearCart(): void {
    this._items.set([]);
  }

  /**
   * Obtiene la cantidad actual que tiene un producto en el carrito.
   */
  getItemQuantity(productId: string): number {
    return this._items().find(i => i.product.id === productId)?.quantity ?? 0;
  }
}
