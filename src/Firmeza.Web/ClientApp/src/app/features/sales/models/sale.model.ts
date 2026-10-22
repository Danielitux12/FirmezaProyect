import { SaleDetailDto, CreateSaleDetailDto } from '../../sale-details/models/sale-detail.model';

export interface SaleDto {
  id: string;
  date: string;
  total: number;
  clientId: string;
  clientName?: string | null;
  clientEmail?: string | null;
  receiptPdfUrl?: string | null;
  saleDetails: SaleDetailDto[];
}

export interface CreateSaleDto {
  clientId: string;
  saleDetails: CreateSaleDetailDto[];
}
