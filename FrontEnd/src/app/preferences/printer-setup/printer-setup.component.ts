import { Component, OnInit } from '@angular/core';
import { BsModalService } from 'ngx-bootstrap';
import { PrinterSetupModalComponent } from './printer-setup-modal/printer-setup-modal.component';
import { ApiService } from 'src/app/services/api.service';

@Component({
  selector: 'app-printer-setup',
  templateUrl: './printer-setup.component.html',
  styleUrls: ['../preferences.component.scss', './printer-setup.component.css']
})
export class PrinterSetupComponent implements OnInit {

  Printers: any[] = [];
  sizes: Array<any> = [];

  pcsVariables = [
    5,
    10,
    20
  ];

  Criteria ={
    TextSearch:null, 
    Id:null,
    printJobStatus:null,
    UserId:null,
    PrinterId:null,
    Page: 1,
    Limit: 10,
    SortBy: 'Id',
    Ascending :false,
    paginate: false
  };

  itemsPerPage = 10;
  currentPage = 1;
  TotalCount = 0;
  lastSynced;
  syncing: boolean = false;
  loading: boolean = false;
  constructor(private modalService: BsModalService, private api: ApiService) { }

  ngOnInit() {
    this.getPrinters();
    this.GetLastSynced();
    this.api.GetPrinterSizes(success => {
      this.sizes = success;

    }, error => {
      console.log(error);
    });
  }

  editPrinterSetup(printer) {
    const initialState = {
      printer: {...printer},
      sizes: this.sizes
    };

    const bsModalRef = this.modalService.show(PrinterSetupModalComponent, {initialState});
    bsModalRef.content.onClose.subscribe(result => {
      if(result.success) {
        this.getPrinters();
      }
    });
  }

  getPrinters(){
    this.loading = true;
    this.api.GetPrinters(this.Criteria,data => {
      this.Printers = data.List;
      this.TotalCount = data.TotalCount
      this.loading = false;

      this.Printers.forEach(element => {
        if(!element.Size) {
          element.Size = {};
        }
      });

    }, error => {
      console.log(error);
      this.loading = false;
    });
  }

  paginate(event: any) {
    //console.log('paginate', event);
    this.Criteria.Page = event.page;
    this.Criteria.Limit = event.itemsPerPage;
    this.getPrinters();
  }

  setPcsOnpage(pcs) {
    this.Criteria.Limit = pcs;
    this.getPrinters();
  }

  GetLastSynced(){ 
    this.api.GetLastSync("SyncPrinters", data => {
      this.lastSynced = data;
    }, error=>{
      console.log(error);
    })
  }
Sync(){
  this.syncing = true
  this.api.syncPrinters( data => {
    this.syncing = false
    this.GetLastSynced()
    this.getPrinters()
  }, error=>{
    this.syncing = false;
   
  })
}

SortBy(sort){

  if(sort == this.Criteria.SortBy) {
    this.Criteria.Ascending = !this.Criteria.Ascending;
    
  } else {
    this.Criteria.Ascending = true;
  }

  this.Criteria.SortBy = sort;
  this.getPrinters(); 
}

}
