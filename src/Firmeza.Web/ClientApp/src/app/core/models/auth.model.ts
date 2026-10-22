export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  email: string;
  password: string;
  role?: string;
  name?: string;
  phone?: string;
  address?: string;
  clientId?: string | null;
}

export interface AuthResponseDto {
  token: string;
  email: string;
  role: string;
  clientId?: string | null;
  expiresAt: string;
}

export interface UserSession {
  email: string;
  role: 'Admin' | 'Client' | string;
  clientId?: string | null;
}
