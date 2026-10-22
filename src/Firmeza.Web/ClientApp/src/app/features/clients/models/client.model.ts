export interface ClientDto {
  id: string;
  name: string;
  email: string;
  phone: string;
  address: string;
  isActive: boolean;
}

export interface CreateClientDto {
  name: string;
  email: string;
  phone: string;
  address: string;
  password?: string;
}
