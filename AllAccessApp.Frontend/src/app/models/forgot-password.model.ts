export interface ForgotPasswordModel {
  email: string;
}

export interface VerifyOtpModel {
  email: string;
  otp: string;
}

export interface ResetPasswordModel {
  email: string;
  newPassword: string;
  confirmPassword: string;
}