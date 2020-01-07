import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { BehaviorSubject, combineLatest, merge } from "rxjs";
import { isPlatformBrowser } from "@angular/common";
import { NavigationEnd, Router } from "@angular/router";
import { filter, map } from "rxjs/operators";
import { ResizeService } from "./window-resize.service";

@Injectable({
  providedIn: 'root'
})
export class SidebarService {
  isOpened$ = new BehaviorSubject(this.isSidebarOpened());
  private pageChange$ = this.router.events.pipe(filter(e => e instanceof NavigationEnd));
  isHamburgerShown$ = this.pageChange$.pipe(
    map((e: NavigationEnd) => this.isPageWithHamburger(e.urlAfterRedirects))
  );

  constructor(@Inject(PLATFORM_ID) private platform: Object,
              private router: Router,
              private resizeService: ResizeService) {
    merge(resizeService.onResize$, this.pageChange$).subscribe(() => this.isOpened$.next(this.isSidebarOpened()));
  }

  toggle() {
    this.isOpened$.next(!this.isOpened$.value);
  }

  isPageWithHamburger(pageName: string) {
    const pagesWithHamburger = ['preferences'];
    return pagesWithHamburger.some(p => pageName.includes(p));
  }

  private isSidebarOpened() {
    return this.isWindowWide() && this.isPageWithHamburger(this.router.url);
  }

  private isWindowWide() {
    if (isPlatformBrowser(this.platform) && window) {
      return window.innerWidth > 1200;
    } else {
      return true;
    }
  }
}
