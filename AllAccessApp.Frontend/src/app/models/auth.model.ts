export interface LoginModel{
  email:string;
  password:string;
}

export interface RegisterModel{
  name:string;
  email:string;
  password:string;
}

export interface AuthResponse{
  success:boolean;
  message:string;
  token:string;
  userId:number;
  email:string;
  name:string;
}