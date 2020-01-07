import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';

@Component({
  selector: 'app-printer-settings',
  templateUrl: './printer-settings.component.html',
  styleUrls: ['./printer-settings.component.css']
})

export class PrinterSettingsComponent implements OnInit {

  sizes: Array<any> = [{}];
  selectedIndex = 0;
  loading: boolean = false;
  
  constructor(private api: ApiService) {
  }

  ngOnInit() {
    this.loadSizes(true);
  }

  loadSizes(selectFirst) {
    this.loading = true;
    this.api.GetPrinterSizes(success => {

      this.sizes = success;
      this.loading = false;
    }, error => {
      this.loading = false;
    });
  }

  save() {
    this.loading = true;
    this.api.UpdatePrinterSize(this.sizes[this.selectedIndex], success => {

      this.loadSizes(false);
      alert('Saved');
      this.loading = false;

    }, error => {
      alert(error.Messages.join(','));
      this.loading = false;
    });
  }
}
