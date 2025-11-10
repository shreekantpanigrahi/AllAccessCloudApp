import { Injectable } from '@angular/core';
import Swal from 'sweetalert2';
@Injectable({
  providedIn: 'root'
})
export class AlertService {

  success(title:string, text?:string){
    Swal.fire({
      icon:'success',
      title,
      text,
      confirmButtonText:'OK',
      confirmButtonColor:'#0d6efd',
      timer: 3000,
      timerProgressBar:true,
      toast:true,
      position:'top-end',
      showConfirmButton: false
    });
  }

  error(title:string, text?:string): void{
    Swal.fire({
      icon:'error',
      title,
      text,
      confirmButtonText:'OK',
      confirmButtonColor:'#dc3545'
    });
  }

  warning(title: string, text?:string): void{
    Swal.fire({
      icon: 'warning',
      title,
      text,
      confirmButtonText: 'Got it',
      confirmButtonColor: '#ffc107'
    });
  }

  async confirm(title: string, text:string, confirmText='Yes', cancelText='Cancel'): Promise<boolean>{
    const result = await Swal.fire({
      title,
      text,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: confirmText,
      cancelButtonText: cancelText,
      confirmButtonColor: '#0d6efd',
      cancelButtonColor: '#6c757d'
    });
    return result.isConfirmed;
  }

  loading(title: string = 'Processing...'): void {
    Swal.fire({
      title,
      allowOutsideClick: false,
      didOpen: () => {
        Swal.showLoading();
      }
    });
  }

  close(): void {
    Swal.close();
  }
  
}
