export interface LoginDto {
  userName: string;
  password: string;
  captcha: string;
}

export interface UserProfileDto {
  userName: string | null;
  userEMail: string | null;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
}
