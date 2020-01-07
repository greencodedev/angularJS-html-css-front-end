import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { BsModalService } from 'ngx-bootstrap';
import { AddUserModalComponent } from './add-user-modal/add-user-modal.component';
import { __core_private_testing_placeholder__ } from '@angular/core/testing';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['../preferences.component.scss', './users.component.css']
})
export class UsersComponent implements OnInit {
  pcsVariables = [
    5,
    10,
    20
  ];
  Users: any[];
  loading = false;
  search: any = {
    Status: true
  };

  itemsPerPage = 10;
  currentPage = 1;

  selectedUser = {};

  constructor(private api: ApiService, private modalService: BsModalService) {
  }

  ngOnInit() {
    this.getUsers();
  }

  getUsers() {
    this.loading = true;
    this.api.GetUsers(this.search,
      data => {
        this.Users = data;
        this.loading = false;
      },
      error => {
        this.loading = false;
        alert(error.Messages.join(','));

      }
    );
  }

  SelectUser(user) {
    this.selectedUser = user;
    this.api.ForgetPassword(
      user, data => {
        alert('An E-mail was sent to the user with instructions on how to reset the password');
      }, error => {
        alert(error.Messages.join(','));
      }
    );
  }

  invokeAddUserModal(user = null) {

    var init: any = {};

    if (user) {
      init.user = {...user};
    }

    const modal = this.modalService.show(AddUserModalComponent, {initialState:init, class: 'wide-modal'});

    modal.content.onClose.subscribe(result => {
      if(result.success) {
        this.getUsers();
      }
    });
  }

  UpdateUser(user, status, showSuccessAlert = true){
    this.loading = true;
    var userCopy = {...user};
    userCopy.Active = status;
    
    this.api.UpdateUser(userCopy, 
      data =>{
        this.loading = false;
        this.getUsers();
        if (showSuccessAlert) {
          alert("success");
        }
      },
      error=>{
        this.loading = false;
        alert(error.Messages.join(','));
      }
      )
  }

  ToggleUserActivation(user) {
    this.UpdateUser(user, !!user.Active, false);
  }
}
