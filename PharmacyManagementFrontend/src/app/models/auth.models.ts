export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  name: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  status: string;
}

export interface User {
  userId: string;
  email: string;
  role: string;
  status?: string;
}