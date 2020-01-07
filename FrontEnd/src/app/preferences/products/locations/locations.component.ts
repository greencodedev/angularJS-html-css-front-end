import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';

@Component({
  selector: 'app-locations',
  templateUrl: './locations.component.html',
  styleUrls: ['./locations.component.css']
})
export class LocationsComponent implements OnInit {
  pcsVariables = [
    5,
    10,
    20
  ];

  Criteria ={
    TextSearch:null, 
    Id:null,
    Status:true,
    UserId:null,
    PrinterId:null,
    Page: 1,
    Limit: 10,
    SortBy: 's.Name',
    Ascending :true,
    paginate: false,
    OnlySublocation: true
  };

  itemsPerPage = 10;
  currentPage = 1;
  TotalCount = 0;

  Sublocations: any[];
  loading = false;
  syncing: boolean = false;
  lastSynced;

  constructor(private api: ApiService) {
  }

  ngOnInit() {
    this.GetSublocations();
    this.GetLastSynced();
  }

  GetSublocations() {
    this.loading = true;
    var $this = this;

    this.api.GetSublocations(
      $this.Criteria,
      1,//channel id here
      data => {
        $this.Sublocations = data;
        this.loading = false;
      },
      error => {
        this.loading = false;
        alert(error.Messages.join(','));
      }
    );
  }

  Sync(){
    this.syncing = true;
    this.api.Sync(1, // channel id goes here
      data => {
      this.GetSublocations();
      this.GetLastSynced();
      this.syncing = false;

    }, error => {
      alert("there was an error while trying to sync");
      this.syncing = false;
      console.log(error);
    })
  }

  GetLastSynced() {
    this.api.GetLastSync("SyncProducts",
      data => {
        this.lastSynced = data;
      },
      error => console.log(error)
    );
  }

  saveStatus() {
    var $this = this;

    this.api.UpdateSublocations($this.Sublocations,
      1,// channel id here
       data => {
        //$this.Sublocations = data;
        $this.GetSublocations();
      },
      error => {
        this.loading = false;
        alert(error.Messages.join(','));
      }
    );
  }

  paginate(event: any) {
    //console.log('paginate', event);
    this.Criteria.Page = event.page;
    this.Criteria.Limit = event.itemsPerPage;
    this.GetSublocations();
  }

  setPcsOnpage(pcs) {
    this.Criteria.Limit = pcs;
    this.GetSublocations();
  }

  SortBy(sort){

    if(sort == this.Criteria.SortBy) {
      this.Criteria.Ascending = !this.Criteria.Ascending;
      
    } else {
      this.Criteria.Ascending = true;
    }
  
    this.Criteria.SortBy = sort;
    this.GetSublocations(); 
  }

}
