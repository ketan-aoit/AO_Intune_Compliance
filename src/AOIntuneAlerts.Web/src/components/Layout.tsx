import type { ReactNode } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  makeStyles,
  tokens,
  Button,
  Text,
  Avatar,
  Menu,
  MenuTrigger,
  MenuPopover,
  MenuList,
  MenuItem,
  Divider,
} from '@fluentui/react-components';
import {
  Navigation20Regular,
  Home20Regular,
  Desktop20Regular,
  ShieldCheckmark20Regular,
  Alert20Regular,
  Settings20Regular,
  SignOut20Regular,
} from '@fluentui/react-icons';
import { useAuth } from '../auth';
import { useUIStore } from '../store/uiStore';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    height: '100vh',
  },
  sidebar: {
    width: '250px',
    backgroundColor: tokens.colorNeutralBackground2,
    borderRight: `1px solid ${tokens.colorNeutralStroke1}`,
    display: 'flex',
    flexDirection: 'column',
    transition: 'width 0.2s ease',
  },
  sidebarCollapsed: {
    width: '60px',
  },
  sidebarHeader: {
    padding: tokens.spacingVerticalM,
    display: 'flex',
    alignItems: 'center',
    gap: tokens.spacingHorizontalM,
    borderBottom: `1px solid ${tokens.colorNeutralStroke1}`,
  },
  sidebarNav: {
    flex: 1,
    padding: tokens.spacingVerticalS,
  },
  navItem: {
    display: 'flex',
    alignItems: 'center',
    gap: tokens.spacingHorizontalM,
    padding: `${tokens.spacingVerticalS} ${tokens.spacingHorizontalM}`,
    borderRadius: tokens.borderRadiusMedium,
    cursor: 'pointer',
    color: tokens.colorNeutralForeground1,
    textDecoration: 'none',
    marginBottom: tokens.spacingVerticalXS,
    '&:hover': {
      backgroundColor: tokens.colorNeutralBackground1Hover,
    },
  },
  navItemActive: {
    backgroundColor: tokens.colorBrandBackground2,
    color: tokens.colorBrandForeground1,
  },
  main: {
    flex: 1,
    display: 'flex',
    flexDirection: 'column',
    overflow: 'hidden',
  },
  header: {
    height: '56px',
    padding: `0 ${tokens.spacingHorizontalL}`,
    borderBottom: `1px solid ${tokens.colorNeutralStroke1}`,
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
  },
  content: {
    flex: 1,
    overflow: 'auto',
    padding: tokens.spacingHorizontalL,
    backgroundColor: tokens.colorNeutralBackground1,
  },
});

interface LayoutProps {
  children: ReactNode;
}

const navItems = [
  { path: '/', label: 'Dashboard', icon: <Home20Regular /> },
  { path: '/devices', label: 'Devices', icon: <Desktop20Regular /> },
  { path: '/compliance-rules', label: 'Compliance Rules', icon: <ShieldCheckmark20Regular />, adminOnly: true },
  { path: '/alerts', label: 'Alerts', icon: <Alert20Regular /> },
  { path: '/settings', label: 'Settings', icon: <Settings20Regular />, adminOnly: true },
];

export function Layout({ children }: LayoutProps) {
  const styles = useStyles();
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout, isAdmin } = useAuth();
  const { sidebarCollapsed, toggleSidebar } = useUIStore();

  const filteredNavItems = navItems.filter((item) => {
    if (item.adminOnly) return isAdmin();
    return true;
  });

  return (
    <div className={styles.container}>
      <aside className={`${styles.sidebar} ${sidebarCollapsed ? styles.sidebarCollapsed : ''}`}>
        <div className={styles.sidebarHeader}>
          <Button
            appearance="subtle"
            icon={<Navigation20Regular />}
            onClick={toggleSidebar}
          />
          {!sidebarCollapsed && (
            <Text weight="semibold">Intune Compliance</Text>
          )}
        </div>
        <nav className={styles.sidebarNav}>
          {filteredNavItems.map((item) => (
            <div
              key={item.path}
              className={`${styles.navItem} ${location.pathname === item.path ? styles.navItemActive : ''}`}
              onClick={() => navigate(item.path)}
            >
              {item.icon}
              {!sidebarCollapsed && <Text>{item.label}</Text>}
            </div>
          ))}
        </nav>
      </aside>
      <main className={styles.main}>
        <header className={styles.header}>
          <Text size={500} weight="semibold">
            {navItems.find((item) => item.path === location.pathname)?.label || 'Dashboard'}
          </Text>
          <Menu>
            <MenuTrigger disableButtonEnhancement>
              <Button appearance="subtle" icon={<Avatar name={user?.name || 'User'} size={32} />} />
            </MenuTrigger>
            <MenuPopover>
              <MenuList>
                <MenuItem disabled>
                  <Text weight="semibold">{user?.name}</Text>
                </MenuItem>
                <MenuItem disabled>
                  <Text size={200}>{user?.username}</Text>
                </MenuItem>
                <Divider />
                <MenuItem icon={<SignOut20Regular />} onClick={logout}>
                  Sign out
                </MenuItem>
              </MenuList>
            </MenuPopover>
          </Menu>
        </header>
        <div className={styles.content}>{children}</div>
      </main>
    </div>
  );
}
