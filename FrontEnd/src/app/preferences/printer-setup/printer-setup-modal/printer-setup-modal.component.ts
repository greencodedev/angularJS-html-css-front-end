import { Component, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap';
import { ApiService } from 'src/app/services/api.service';
import { Subject } from 'rxjs';


@Component({
  selector: 'app-printer-setup-modal',
  templateUrl: './printer-setup-modal.component.html',
  styleUrls: ['./printer-setup-modal.component.css']
})
export class PrinterSetupModalComponent implements OnInit {

  sizes: Array<any> = [];
  printer: any = {};
  onClose: Subject<any>;
  
  constructor(public bsModalRef: BsModalRef, private api: ApiService) {}

  ngOnInit() {
    //console.log(this.printer);
    this.onClose = new Subject();
  }

  save(printer) {
    if((printer.CustomName == null || printer.CustomName == '') && printer.Active) {
      printer.Active = false;
      alert("Please add a custom printer name before activating the printer.");
    } else {
      this.api.UpdatePrinter(printer, data => {

        this.close(true);
      
      }, error =>{
        alert(error.Messages.join(','))
      }
    )};
  }

  close(success: boolean) {
    this.onClose.next({
      success: success
    });
    this.bsModalRef.hide();
  }
}
