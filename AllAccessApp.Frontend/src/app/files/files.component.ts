import { NgIf, NgForOf } from '@angular/common';
import { ChangeDetectorRef, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FileItemDto, FileListResponse, FileUploadResponse } from '../models/file.model';
import { FilesService } from '../services/files.service';
import { AlertService } from '../services/alert.service';
import { ProfileService } from '../services/profile.service';

@Component({
  selector: 'app-files',
  standalone: true,
  imports: [NgIf, NgForOf],
  templateUrl: './files.component.html',
  styleUrl: './files.component.css'
})
export class FilesComponent implements OnInit {
  files:FileItemDto[]=[];
  totalFiles=0;
  loading=true;
  uploadSuccess= false;
  uploadError='';
  
  @ViewChild('fileInput') fileInput!: ElementRef;
  

  constructor(private filesService: FilesService, private cdr: ChangeDetectorRef, private alertService:AlertService, private profileService: ProfileService){}

  ngOnInit():void{
    this.loadFiles();
  }

  loadFiles(): void {
    this.loading = true;
    this.alertService.loading("Loading files...");
    this.filesService.getUserFiles().subscribe({
      next: (data: FileListResponse) => {
        this.files = data.files || [];
        this.totalFiles = this.files.length;
        this.loading = false;
        this.alertService.close();
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.loading = false;
        this.alertService.close();
        this.alertService.error("Failed to load files", err);
      }
    });
  }

  downloadFile(fileId: number): void{
    this.alertService.loading("Downloading...");
    this.filesService.downloadFile(fileId).subscribe({
      next: (blob)=>{
        this.alertService.close();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');

        a.href=url;
        a.download=this.getFileNameById(fileId);

        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      },
      error:()=>{
        this.alertService.close();
        this.alertService.error("Download failed!!!");
      }
    });
  }

  viewFile(fileId : number):void{
    this.alertService.loading("Loading Please wait");
    this.filesService.viewFile(fileId).subscribe({
      next:(blob)=>{
        this.alertService.close();
        const url = window.URL.createObjectURL(blob);
        window.open(url,'_blank');
      },
      error:()=>{
        this.alertService.close();
        this.alertService.error("Preview not available");
      }
    });
  }

  async deleteFile(fileId: number):Promise<void>{
    const confirmed = await this.alertService.confirm(
      'Delete File?',
      'Are you sure you want to delete the file? This cannot be undone',
      'Delete',
      'Cancel'
    )

    if(!confirmed) return;

    this.filesService.deleteFile(fileId).subscribe({
      next: ()=>{
        this.alertService.success('Deleted!',"The file deleted successfully!!!");
        this.loadFiles();
      },
      error:(err)=>{
        const message = err.error?.message || 'Could not delete file.';
        this.alertService.error('Delete Failed', message);
      }
    });
  }

  //Helper: Get file name by ID for download 
  private getFileNameById(id: number): string{
    const file = this.files.find(f=>f.id===id);
    return file?.originalName || 'file';
  }

  //Format ISO date to readable format
  formatDateTime(date:string):string{
    return new Date(date).toLocaleDateString('en-US',{
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  // Return icon based on file type
  getFileIcon(fileType: string): string {
    if (!fileType) return 'assets/icons/MyFilesIcon/file.png';

    if (fileType.startsWith('image/')) return 'assets/icons/MyFilesIcon/img.png';
    if (fileType === 'application/pdf') return 'assets/icons/MyFilesIcon/pdf.png';
    if (fileType.includes('word') || fileType.includes('doc')) return 'assets/icons/MyFilesIcon/doc.png';
    if (fileType.includes('excel') || fileType.includes('sheet')) return 'assets/icons/MyFilesIcon/xls.png';
    if (fileType.includes('powerpoint') || fileType.includes('presentation')) return 'assets/icons/MyFilesIcon/ppt.png';
    if (fileType.startsWith('video/')) return 'assets/icons/MyFilesIcon/video.png';
    if (fileType.startsWith('audio/')) return 'assets/icons/MyFilesIcon/audio.png';
    return 'assets/icons/MyFilesIcon/file.png';
  }

  openFileInput():void{
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: any): void{
    const file = event.target.files[0];
    this.alertService.warning('No file selected', 'Please choose a valid file.')
    if(!file) return;

    // Clear input so selecting the same file triggers change again
    event.target.value = null;
    this.uploadFile(file);
  }

  private uploadFile(file:File): void{
      this.loading=true;
      this.uploadSuccess=false;
      this.uploadError='';
      this.alertService.loading('Uploading...');
  
      this.filesService.uploadFile(file).subscribe({
        next: async (response: FileUploadResponse)=>{
          this.loading = false;
          this.alertService.close();  // Close loading popup
          this.loadFiles();
          this.uploadSuccess = true;
          this.alertService.success('Uploaded!', `${response.fileName} was saved successfully.`);

          try {
            await this.profileService.reloadProfile();
            console.log('Profile reloaded with updated storage');
          } catch (err) {
            console.error('Failed to reload profile', err);
          }
        },
        error:(err)=>{
          this.loading=false;
          this.alertService.close();
          this.loadFiles();
          this.uploadError= err.error?.message || 'Upload failed. Please try again.';
          const message = this.uploadError;
  
          this.alertService.error('Upload Failed', message);
        }
      });
    }

}
