import { Component, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ApiService } from 'src/app/services/api.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.css']
})

export class ModalComponent implements OnInit {

  printers: Array<any> =[];

  listOpened = false;
  sentToPrint = false;
  selectedPrinterDisplay = '';
  selectedPrinter;
  product;
  fromConvert
  printjob;
  isPrinting: boolean = false;
  quantity = 1;

  constructor(
    public modalRef: BsModalRef,
    private api: ApiService
  ) {
  }

  ngOnInit() {
    //console.log('product', this.product);
    //console.log('printers', this.printers);
    
  }

  getPrinters() {
    return this.printers.filter(x => x.Active == true && x.IsAssigned == true);
  }

  switchList() {
    //console.log('swithced');
    this.listOpened = !this.listOpened;
  }

  selectPrinter(printer) {
    const printerSize = printer.Size && printer.Size.Name && `( ${printer.Size.Name} )` || '';
    this.selectedPrinterDisplay = `${printer.CustomName || printer.Name} ${printerSize}`;
    this.selectedPrinter = printer;
    // this.switchList()
  }

  print() {
    const printJob = {
      Printer: this.selectedPrinter,
      Product: this.product,
      Quantity: this.quantity,
      FromConvert: this.fromConvert
    };

    this.api.CreatePrintJob(
      printJob,
      data => {
        this.isPrinting = true;
        this.printjob = data;
        this.printStatus(1);
      },
      error => {
        console.log(error);
        alert(error.Messages.join(','));
      }
    );
  }

  private printStatus(count) {

    this.api.GetPrintJob(this.printjob.PrintNodePrintJobId, sucess => {

      if(sucess.Status == 'Success') {
        this.sentToPrint = true;

      } else if(sucess.Status == 'Error') {
        this.isPrinting = false;
        alert('Error printing');

      } else {

        if(count < 10) {

          setTimeout(() => {
            this.printStatus(count + 1);
          }, 1000);
        } else {

          this.isPrinting = false;
          alert('This print job is taking to long to print, Please check your printer and try again.');
        }
      }

    }, error => {

      console.log(error);
      this.isPrinting = false;
      alert('Error printing');
    });
  }
}
